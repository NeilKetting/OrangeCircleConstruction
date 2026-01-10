using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IOrderService _orderService;

        // Child ViewModels
        public OrderDashboardViewModel DashboardVM { get; }
        public OrderListViewModel OrderListVM { get; } // Renamed for clarity, was ListVM
        public SupplierListViewModel SupplierListVM { get; }
        public InventoryViewModel InventoryListVM { get; } // Was InventoryVM
        public CreateOrderViewModel OrderDetailVM { get; } 
        public SupplierDetailViewModel SupplierDetailVM { get; }

        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private string _activeTab = "Dashboard"; // Dashboard, Orders, Sales, Returns, Suppliers, Inventory

        // Popups
        [ObservableProperty]
        private bool _isOrderDetailVisible;

        [ObservableProperty]
        private bool _isSupplierDetailVisible;

        public OrderViewModel(
            OrderDashboardViewModel dashboardVM,
            OrderListViewModel listVM,
            CreateOrderViewModel createOrderVM,
            InventoryViewModel inventoryVM,
            SupplierListViewModel supplierListVM,
            SupplierDetailViewModel supplierDetailVM,
            IDialogService dialogService,
            IOrderService orderService)
        {
            DashboardVM = dashboardVM;
            OrderListVM = listVM;
            OrderDetailVM = createOrderVM; 
            InventoryListVM = inventoryVM;
            SupplierListVM = supplierListVM;
            SupplierDetailVM = supplierDetailVM;
            _dialogService = dialogService;
            _orderService = orderService;

            _currentView = DashboardVM;
            
            // Wire up popup events
            OrderDetailVM.CloseRequested += (s, e) => IsOrderDetailVisible = false;
            
            SupplierListVM.AddSupplierRequested += (s, e) => 
            {
                SupplierDetailVM.Load(null); // Add Mode
                IsSupplierDetailVisible = true;
            };

            SupplierDetailVM.CloseRequested += (s, e) => IsSupplierDetailVisible = false;
            SupplierDetailVM.Saved += async (s, e) => await SupplierListVM.LoadData();
        }

        [RelayCommand]
        public void NavigateToDashboard() => SetActiveTab("Dashboard");

        [RelayCommand]
        public void NavigateToAllOrders() => SetActiveTab("Orders");

        [RelayCommand]
        public void NavigateToInventory() => SetActiveTab("Inventory");

        [RelayCommand]
        public void NavigateToCreateOrder() => OpenNewOrder();

        [RelayCommand]
        public void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
            switch (tabName)
            {
                case "Dashboard":
                    CurrentView = DashboardVM;
                    break;
                case "Orders": // Purchase Orders
                    // OrderListVM.FilterByType(OrderType.PurchaseOrder); // Future: Implement Filter
                    CurrentView = OrderListVM;
                    break;
                case "Sales":
                    // OrderListVM.FilterByType(OrderType.SalesOrder);
                    CurrentView = OrderListVM;
                    break;
                case "Returns":
                    CurrentView = OrderListVM;
                    break;
                case "Suppliers":
                    CurrentView = SupplierListVM;
                    _ = SupplierListVM.LoadData();
                    break;
                case "Inventory":
                    CurrentView = InventoryListVM;
                    break;
                default:
                    CurrentView = DashboardVM;
                    break;
            }
        }
        
        [RelayCommand]
        public void OpenNewOrder()
        {
             // Determine type based on Active Tab?
             // For now default to PO or ask user in popup?
             // Let's assume PO for now.
             OrderDetailVM.LoadData(); // Reset
             IsOrderDetailVisible = true;
        }
    }
}

