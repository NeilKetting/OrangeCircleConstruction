using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderDashboardViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<OrderDashboardViewModel> _logger;

        [ObservableProperty]
        private int _ordersThisMonth;
        
        [ObservableProperty]
        private int _pendingDeliveries;
        
        [ObservableProperty]
        private int _lowStockItemsCount;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<Order> RecentOrders { get; } = new();
        public ObservableCollection<InventoryItem> LowStockItems { get; } = new();

        public OrderDashboardViewModel(IOrderService orderService, IInventoryService inventoryService, ILogger<OrderDashboardViewModel> logger)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _logger = logger;
            _ = LoadData(); 
        }

        public async Task LoadData()
        {
            try
            {
                IsBusy = true;
                
                // Parallel Fetch
                var ordersTask = _orderService.GetOrdersAsync();
                var inventoryTask = _inventoryService.GetInventoryAsync();

                await Task.WhenAll(ordersTask, inventoryTask);

                var orders = await ordersTask;
                var inventory = await inventoryTask;

                ProcessOrders(orders);
                ProcessInventory(inventory);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ProcessOrders(List<Order> orders)
        {
            // Stats
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            
            OrdersThisMonth = orders.Count(o => o.OrderDate >= startOfMonth);
            PendingDeliveries = orders.Count(o => o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartialDelivery);

            // Recent Orders (Top 5)
            RecentOrders.Clear();
            var recent = orders.OrderByDescending(o => o.OrderDate).Take(5);
            foreach(var o in recent) RecentOrders.Add(o);
        }

        private void ProcessInventory(List<InventoryItem> inventory)
        {
            LowStockItems.Clear();
            var lowStock = inventory.Where(i => i.QuantityOnHand <= i.ReorderPoint).ToList();
            
            LowStockItemsCount = lowStock.Count;
            
            foreach(var item in lowStock.Take(5)) // Show top 5 alerts
            {
                LowStockItems.Add(item);
            }
        }
    }
}
