using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;

namespace OCC.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IInventoryService _inventoryService;
        private readonly IAuthService _authService;

        public OrderService(HttpClient httpClient, IInventoryService inventoryService, IAuthService authService)
        {
            _httpClient = httpClient;
            _inventoryService = inventoryService;
            _authService = authService;
        }

        private void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            EnsureAuthorization();
            return await _httpClient.GetFromJsonAsync<List<Order>>("api/Orders") ?? new List<Order>();
        }

        public async Task<Order?> GetOrderAsync(Guid id)
        {
             EnsureAuthorization();
             return await _httpClient.GetFromJsonAsync<Order>($"api/Orders/{id}");
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync("api/Orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>() ?? order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
             EnsureAuthorization();
             var response = await _httpClient.PutAsJsonAsync($"api/Orders/{order.Id}", order);
             response.EnsureSuccessStatusCode();
        }

        public async Task ReceiveOrderAsync(Order order, List<OrderLine> updatedLines)
        {
            // 1. Process Inventory Updates
            // Only for Inbound orders (PO, Returns)
            bool isInbound = order.OrderType == OrderType.PurchaseOrder || order.OrderType == OrderType.ReturnToInventory;

            foreach (var updatedLine in updatedLines)
            {
                var originalLine = order.Lines.FirstOrDefault(l => l.Id == updatedLine.Id);
                if (originalLine == null) continue;

                double delta = updatedLine.QuantityReceived - originalLine.QuantityReceived;

                // Update the Order Line in memory object (so it's ready for save)
                originalLine.QuantityReceived = updatedLine.QuantityReceived;

                if (isInbound && delta > 0 && originalLine.InventoryItemId.HasValue)
                {
                    try 
                    {
                        var item = await _inventoryService.GetInventoryItemAsync(originalLine.InventoryItemId.Value);
                        if (item != null)
                        {
                            item.QuantityOnHand += delta;
                            await _inventoryService.UpdateItemAsync(item);
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to update inventory for {originalLine.ItemCode}: {ex.Message}");
                        // Continue? Or throw? For now log and continue to prioritize saving the Order state.
                    }
                }
            }

            // 2. Update Status
            // Check if all lines are fully received
            bool allComplete = order.Lines.All(l => l.QuantityReceived >= l.QuantityOrdered);
            bool anyReceived = order.Lines.Any(l => l.QuantityReceived > 0);

            if (allComplete) order.Status = OrderStatus.Completed;
            else if (anyReceived) order.Status = OrderStatus.PartialDelivery;
            // Else remains as is (Ordered/Draft)

            // 3. Save Order
            await UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(Guid id)
        {
             var response = await _httpClient.DeleteAsync($"api/Orders/{id}");
             response.EnsureSuccessStatusCode();
        }
    }
}
