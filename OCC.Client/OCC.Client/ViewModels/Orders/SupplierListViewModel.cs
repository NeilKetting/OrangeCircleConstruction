using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using OCC.Client.ViewModels.Core;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Orders
{
    public partial class SupplierListViewModel : ViewModelBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private List<Supplier> _allSuppliers = new();

        public ObservableCollection<Supplier> Suppliers { get; } = new();

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isBusy;
        
        [ObservableProperty]
        private Supplier? _selectedSupplier;

        private readonly ILogger<SupplierListViewModel> _logger;

        public SupplierListViewModel(ISupplierService supplierService, IOrderService orderService, IDialogService dialogService, ILogger<SupplierListViewModel> logger)
        {
            _supplierService = supplierService;
            _orderService = orderService;
            _dialogService = dialogService;
            _logger = logger;
            // LoadSuppliers(); // Called by Host or explicit init
        }

        public async Task LoadData()
        {
            try
            {
                IsBusy = true;
                _allSuppliers = await _supplierService.GetSuppliersAsync();
                FilterSuppliers();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading suppliers");
                await _dialogService.ShowAlertAsync("Error", $"Failed to load suppliers: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event EventHandler? AddSupplierRequested;
        public event EventHandler<Supplier>? EditSupplierRequested;

        [RelayCommand]
        public void AddSupplier()
        {
            AddSupplierRequested?.Invoke(this, EventArgs.Empty);
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterSuppliers();
        }

        private void FilterSuppliers()
        {
            Suppliers.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allSuppliers 
                : _allSuppliers.Where(s => s.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) 
                                        || s.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var s in filtered)
            {
                Suppliers.Add(s);
            }
        }
        
        [RelayCommand]
        private void EditSupplier(Supplier supplier)
        {
             if (supplier == null) return;
             EditSupplierRequested?.Invoke(this, supplier);
        }

        [RelayCommand]
        private async Task DeleteSupplier(Supplier supplier)
        {
             if (supplier == null) return;

             // Dependency Check
             try 
             {
                 var allOrders = await _orderService.GetOrdersAsync();
                 if (allOrders.Any(o => o.SupplierId == supplier.Id))
                 {
                     await _dialogService.ShowAlertAsync("Restricted", $"Cannot delete supplier '{supplier.Name}' because they have associated orders.");
                     return;
                 }
             }
             catch(Exception ex)
             {
                  _logger.LogError(ex, "Failed to check orders during supplier delete");
                  // Fallthrough or stop? Let's safeguard.
                  await _dialogService.ShowAlertAsync("Error", "Could not verify dependencies. Delete aborted.");
                  return;
             }

             var confirm = await _dialogService.ShowConfirmationAsync("Confirm Delete", $"Delete supplier '{supplier.Name}'?");
              if (confirm)
              {
                  try
                  {
                      await _supplierService.DeleteSupplierAsync(supplier.Id);
                      await LoadData();
                  }
                  catch(Exception ex)
                  {
                      await _dialogService.ShowAlertAsync("Error", $"Failed to delete supplier: {ex.Message}");
                  }
              }
        }
    }
}
