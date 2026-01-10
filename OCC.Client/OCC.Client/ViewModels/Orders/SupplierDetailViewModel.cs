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
                    // BankName will be set via logic below
                    AccountNumber = supplier.AccountNumber,
                    BranchCode = supplier.BranchCode
                };
                IsEditMode = true;

                // Map Bank Name
                var dbBankName = supplier.BankName;
                var matched = false;
                
                if (!string.IsNullOrEmpty(dbBankName))
                {
                    foreach (var bank in AvailableBanks)
                    {
                        if (bank == OCC.Shared.Models.BankName.None || bank == OCC.Shared.Models.BankName.Other) continue;

                        if (GetEnumDescription(bank).Equals(dbBankName, StringComparison.OrdinalIgnoreCase))
                        {
                            SelectedBank = bank;
                            matched = true;
                            break;
                        }
                    }
                }

                if (!matched && !string.IsNullOrEmpty(dbBankName))
                {
                    SelectedBank = OCC.Shared.Models.BankName.Other;
                    CustomBankName = dbBankName;
                }
                else if (!matched)
                {
                    SelectedBank = OCC.Shared.Models.BankName.None;
                    CustomBankName = string.Empty;
                }
            }
            else
            {
                // Add
                Supplier = new Supplier();
                IsEditMode = false;
                
                // Default to None
                SelectedBank = OCC.Shared.Models.BankName.None;
                CustomBankName = string.Empty;
            }
        }

        [ObservableProperty]
        private OCC.Shared.Models.BankName _selectedBank = OCC.Shared.Models.BankName.None;

        [ObservableProperty]
        private string _customBankName = string.Empty;

        public bool IsOtherBankSelected => SelectedBank == OCC.Shared.Models.BankName.Other;

        // Expose Bank Enum
        public OCC.Shared.Models.BankName[] AvailableBanks { get; } = Enum.GetValues<OCC.Shared.Models.BankName>();

        partial void OnSelectedBankChanged(OCC.Shared.Models.BankName value)
        {
             OnPropertyChanged(nameof(IsOtherBankSelected));
        }
        
        [RelayCommand]
        private void SetSelectedBank(OCC.Shared.Models.BankName bank)
        {
            SelectedBank = bank;
        }
        
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(System.ComponentModel.DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        [RelayCommand]
        public async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Supplier.Name))
            {
                // Simple validation
                return; 
            }

            // Map Bank Name Back
            if (SelectedBank == OCC.Shared.Models.BankName.None)
            {
                 Supplier.BankName = null;
            }
            else
            {
                 Supplier.BankName = IsOtherBankSelected ? CustomBankName : GetEnumDescription(SelectedBank);
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
