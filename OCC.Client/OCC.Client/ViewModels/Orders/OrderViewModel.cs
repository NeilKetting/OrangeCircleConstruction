using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Shared.Models;
using System;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IDialogService _dialogService;
        private readonly IOrderService _orderService;

        #endregion

        #region Observables

        // Child ViewModels

        [ObservableProperty]
        private OrderMenuViewModel _orderMenu;

        [ObservableProperty]
        private OrderDashboardViewModel _dashboardVM;
        
        [ObservableProperty]
        private OrderListViewModel _orderListVM;
        
        [ObservableProperty]
        private SupplierListViewModel _supplierListVM;
        
        [ObservableProperty]
        private InventoryViewModel _inventoryListVM;
        
        [ObservableProperty]
        private CreateOrderViewModel _orderDetailVM;
        
        [ObservableProperty]
        private ReceiveOrderViewModel _receiveOrderVM;
        
        [ObservableProperty]
        private SupplierDetailViewModel _supplierDetailVM;

        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private string _activeTab = "Dashboard"; // Dashboard, Orders, Sales, Returns, Suppliers, Inventory

        // Popups
        [ObservableProperty]
        private bool _isOrderDetailVisible;
        
        [ObservableProperty]
        private bool _isReceiveOrderVisible;

        [ObservableProperty]
        private bool _isSupplierDetailVisible;

        #endregion

        #region Constructors

        public OrderViewModel(
            OrderMenuViewModel orderMenuVM,
            OrderDashboardViewModel dashboardVM,
            OrderListViewModel listVM,
            CreateOrderViewModel createOrderVM,
            ReceiveOrderViewModel receiveOrderVM,
            InventoryViewModel inventoryVM,
            SupplierListViewModel supplierListVM,
            SupplierDetailViewModel supplierDetailVM,
            IDialogService dialogService,
            IOrderService orderService)
        {
            OrderMenu = orderMenuVM;
            DashboardVM = dashboardVM;
            OrderListVM = listVM;
            OrderDetailVM = createOrderVM;
            ReceiveOrderVM = receiveOrderVM;
            InventoryListVM = inventoryVM;
            SupplierListVM = supplierListVM;
            SupplierDetailVM = supplierDetailVM;
            _dialogService = dialogService;
            _orderService = orderService;

            _currentView = DashboardVM;

            // Wire up popup events
            OrderDetailVM.CloseRequested += (s, e) => IsOrderDetailVisible = false;
            ReceiveOrderVM.CloseRequested += (s, e) => IsReceiveOrderVisible = false;

            OrderListVM.ReceiveOrderRequested += (s, order) => NavigateToReceiveOrder(order);

            // Wire up Menu Navigation
            OrderMenu.TabSelected += (s, tabName) => SetActiveTab(tabName);

            SupplierListVM.AddSupplierRequested += (s, e) =>
            {
                SupplierDetailVM.Load(null); // Add Mode
                IsSupplierDetailVisible = true;
            };

            SupplierDetailVM.CloseRequested += (s, e) => IsSupplierDetailVisible = false;
            SupplierDetailVM.Saved += async (s, e) => await SupplierListVM.LoadData();
        }

        #endregion

        #region Commands

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
            if (tabName == "New Order")
            {
                // Reset Active Tab back to previous if it was just a button click, 
                // OR keep it highlighted if we want. Usually "New Order" is an action, not a tab.
                // For now, launch the popup.
                OpenNewOrder();
                // Optionally reset menu tab visually if desired, but user might want to see 'New Order' selected while popup is open.
                return;
            }

            ActiveTab = tabName;
            switch (tabName)
            {
                case "Dashboard":
                    CurrentView = DashboardVM;
                    _ = DashboardVM.LoadData();
                    break;
                case "All Orders": 
                    // Reset filters?
                    CurrentView = OrderListVM;
                    break;
                case "Suppliers":
                    CurrentView = SupplierListVM;
                    _ = SupplierListVM.LoadData();
                    break;
                case "Inventory":
                    CurrentView = InventoryListVM;
                    // _ = InventoryListVM.LoadData(); // If needed
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

        [RelayCommand]
        public void NavigateToReceiveOrder(Order order)
        {
            ReceiveOrderVM.Initialize(order);
            IsReceiveOrderVisible = true;
        } 

        #endregion
    }
}

