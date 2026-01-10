using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderListViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private List<Order> _allOrders = new();

        public ObservableCollection<Order> Orders { get; } = new();

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isBusy;

        public OrderListViewModel(IOrderService orderService, IDialogService dialogService)
        {
            _orderService = orderService;
            _dialogService = dialogService;
            LoadOrders();
        }

        // public OrderListViewModel() { } // Design-time

        public async void LoadOrders()
        {
            try
            {
                IsBusy = true;
                _allOrders = await _orderService.GetOrdersAsync();
                FilterOrders();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders: {ex.Message}");
                if (_dialogService != null) 
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading orders: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterOrders();
        }

        private void FilterOrders()
        {
            Orders.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allOrders 
                : _allOrders.Where(o => o.OrderNumber.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) 
                                     || o.SupplierName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var order in filtered)
            {
                Orders.Add(order);
            }
        }

        [RelayCommand]
        public async Task DeleteOrder(Order order)
        {
            if (order == null) return;

            try 
            {
                await _orderService.DeleteOrderAsync(order.Id);
                Orders.Remove(order);
                _allOrders.Remove(order);
            }
            catch(Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error deleting order: {ex.Message}");
            }
        }
    }
}
