using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        // Child ViewModels
        public OrderDashboardViewModel DashboardVM { get; }
        public OrderListViewModel ListVM { get; }
        public CreateOrderViewModel CreateOrderVM { get; }
        public InventoryViewModel InventoryVM { get; }

        public OrderViewModel(
            OrderDashboardViewModel dashboardVM,
            OrderListViewModel listVM,
            CreateOrderViewModel createOrderVM,
            InventoryViewModel inventoryVM)
        {
            // Initialize children from DI
            DashboardVM = dashboardVM;
            ListVM = listVM;
            CreateOrderVM = createOrderVM;
            InventoryVM = inventoryVM;

            // Default view
            _currentView = DashboardVM;
            
            // Wire up navigation events from children if needed
        }

        [RelayCommand]
        public void NavigateToDashboard()
        {
            CurrentView = DashboardVM;
        }

        [RelayCommand]
        public void NavigateToAllOrders()
        {
            CurrentView = ListVM;
        }

        [RelayCommand]
        public void NavigateToCreateOrder()
        {
            CurrentView = CreateOrderVM;
        }
        
        [RelayCommand]
        public void NavigateToInventory()
        {
            CurrentView = InventoryVM;
        }
    }
}
