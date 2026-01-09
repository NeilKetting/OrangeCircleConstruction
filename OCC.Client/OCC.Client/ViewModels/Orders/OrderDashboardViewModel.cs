using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.Generic;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderDashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _ordersThisMonth;
        
        [ObservableProperty]
        private int _pendingDeliveries;
        
        [ObservableProperty]
        private int _lowStockItemsCount;

        public ObservableCollection<Order> RecentOrders { get; } = new();
        public ObservableCollection<InventoryItem> LowStockItems { get; } = new();

        public OrderDashboardViewModel()
        {
            LoadMockData();
        }

        private void LoadMockData()
        {
            // Mock Stats
            OrdersThisMonth = 14;
            PendingDeliveries = 3;
            LowStockItemsCount = 5;

            // Mock Recent Orders
            RecentOrders.Add(new Order 
            { 
                OrderNumber = "PO-2026-001", 
                SupplierName = "Builders Warehouse", 
                OrderDate = DateTime.Now.AddDays(-2), 
                ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                DestinationType = OrderDestinationType.Site,
                ProjectName = "Engen Bendor",
                Status = OrderStatus.Ordered
            });
            
            RecentOrders.Add(new Order 
            { 
                OrderNumber = "PO-2026-002", 
                SupplierName = "Timber City", 
                OrderDate = DateTime.Now.AddDays(-5), 
                ExpectedDeliveryDate = DateTime.Now.AddDays(-1), // Late?
                DestinationType = OrderDestinationType.Stock,
                Status = OrderStatus.PartialDelivery
            });
            
             RecentOrders.Add(new Order 
            { 
                OrderNumber = "PO-2026-003", 
                SupplierName = "Plumb It", 
                OrderDate = DateTime.Now.AddDays(-1), 
                ExpectedDeliveryDate = DateTime.Now.AddDays(3),
                DestinationType = OrderDestinationType.Site,
                ProjectName = "Shell Ultra City",
                Status = OrderStatus.Ordered
            });

            // Mock Inventory Alerts
            LowStockItems.Add(new InventoryItem { ProductName = "Cement 50kg", QuantityOnHand = 12, ReorderPoint = 20, Location = "Shed 1" });
            LowStockItems.Add(new InventoryItem { ProductName = "Copper Pipe 15mm", QuantityOnHand = 5, ReorderPoint = 50, Location = "Rack B" });
        }
    }
}
