using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Order>>("api/Orders") ?? new List<Order>();
        }

        public async Task<Order?> GetOrderAsync(Guid id)
        {
             return await _httpClient.GetFromJsonAsync<Order>($"api/Orders/{id}");
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>() ?? order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
             var response = await _httpClient.PutAsJsonAsync($"api/Orders/{order.Id}", order);
             response.EnsureSuccessStatusCode();
        }

        public async Task DeleteOrderAsync(Guid id)
        {
             var response = await _httpClient.DeleteAsync($"api/Orders/{id}");
             response.EnsureSuccessStatusCode();
        }
    }
}
