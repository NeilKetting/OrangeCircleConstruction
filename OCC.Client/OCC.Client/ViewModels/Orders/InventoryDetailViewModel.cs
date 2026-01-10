using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Orders
{
    public partial class InventoryDetailViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IDialogService _dialogService;
        private bool _isEditMode;
        private Guid _editingId;

        public event EventHandler? CloseRequested;
        public event EventHandler? ItemSaved;

        [ObservableProperty]
        private string _title = "Add New Item";

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _category = "General";

        [ObservableProperty]
        private string _location = "Warehouse";

        [ObservableProperty]
        private string _unitOfMeasure = "ea";

        [ObservableProperty]
        private double _quantityOnHand;

        [ObservableProperty]
        private double _reorderPoint;
        
        [ObservableProperty]
        private decimal _averageCost;

        [ObservableProperty]
        private bool _isBusy;

        public System.Collections.ObjectModel.ObservableCollection<string> AvailableCategories { get; } = new();
        public System.Collections.Generic.List<string> AvailableUOMs { get; } = new() { "ea", "m", "kg", "L", "m2", "m3", "box", "roll", "pack" };

        public InventoryDetailViewModel(IInventoryService inventoryService, IDialogService dialogService)
        {
            _inventoryService = inventoryService;
            _dialogService = dialogService;
        }

        public void Load(InventoryItem? item, System.Collections.Generic.List<string>? categories = null)
        {
            AvailableCategories.Clear();
            if (categories != null)
            {
                foreach (var c in categories) AvailableCategories.Add(c);
            }

            if (item == null)
            {
                _isEditMode = false;
                Title = "Add New Item";
                ProductName = "";
                Category = "General"; // Default
                Location = "Warehouse";
                UnitOfMeasure = "ea";
                QuantityOnHand = 0;
                ReorderPoint = 10;
            }
            else
            {
                _isEditMode = true;
                _editingId = item.Id;
                Title = $"Edit {item.ProductName}";
                ProductName = item.ProductName;
                Category = item.Category;
                Location = item.Location;
                UnitOfMeasure = item.UnitOfMeasure;
                QuantityOnHand = item.QuantityOnHand;
                ReorderPoint = item.ReorderPoint;
                AverageCost = item.AverageCost;
            }
        }

        [RelayCommand]
        public async Task Save()
        {
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                await _dialogService.ShowAlertAsync("Validation", "Product Name is required.");
                return;
            }

            try
            {
                IsBusy = true;

                InventoryItem item = new InventoryItem
                {
                    Id = _isEditMode ? _editingId : Guid.NewGuid(),
                    ProductName = ProductName,
                    Category = Category,
                    Location = Location,
                    UnitOfMeasure = UnitOfMeasure,
                    QuantityOnHand = QuantityOnHand,
                    ReorderPoint = ReorderPoint,
                    AverageCost = AverageCost
                };

                if (_isEditMode)
                {
                    await _inventoryService.UpdateItemAsync(item);
                }
                else
                {
                    await _inventoryService.CreateItemAsync(item);
                }

                await _dialogService.ShowAlertAsync("Success", "Inventory item saved successfully.");
                ItemSaved?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to save item: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
