using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Orders
{
    public partial class SupplierDetailViewModel : ViewModelBase
    {
        private readonly ISupplierService _supplierService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private Supplier _supplier = new();

        [ObservableProperty]
        private bool _isEditMode;

        public event EventHandler? CloseRequested;
        public event EventHandler? Saved;

        private readonly ILogger<SupplierDetailViewModel> _logger;

        public SupplierDetailViewModel(ISupplierService supplierService, IDialogService dialogService, ILogger<SupplierDetailViewModel> logger)
        {
            _supplierService = supplierService;
            _dialogService = dialogService;
            _logger = logger;
        }

        public void Load(Supplier? supplier = null)
        {
            if (supplier != null)
            {
                // Edit
                Supplier = new Supplier
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    VatNumber = supplier.VatNumber,
                    BankName = supplier.BankName,
                    AccountNumber = supplier.AccountNumber,
                    BranchCode = supplier.BranchCode
                };
                IsEditMode = true;
            }
            else
            {
                // Add
                Supplier = new Supplier();
                IsEditMode = false;
            }
        }

        [RelayCommand]
        public async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Supplier.Name))
            {
                // Simple validation
                return; 
            }

            try
            {
                if (IsEditMode)
                {
                    await _supplierService.UpdateSupplierAsync(Supplier);
                }
                else
                {
                    await _supplierService.CreateSupplierAsync(Supplier);
                }
                
                Saved?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "Error saving supplier");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", "Failed to save supplier.");
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
