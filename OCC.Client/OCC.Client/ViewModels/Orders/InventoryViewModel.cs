using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Orders
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IDialogService _dialogService;
        private readonly Microsoft.Extensions.Logging.ILogger<InventoryViewModel> _logger;
        private List<InventoryItem> _allItems = new();

        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();

        [ObservableProperty]
        private string _searchQuery = "";

        [ObservableProperty]
        private bool _isBusy;

        public InventoryViewModel(IInventoryService inventoryService, IDialogService dialogService, Microsoft.Extensions.Logging.ILogger<InventoryViewModel> logger)
        {
            _inventoryService = inventoryService;
            _dialogService = dialogService;
            _logger = logger;
            // Fire and forget initialization
            _ = LoadInventoryAsync();
        }

        // public InventoryViewModel() { }

        public async Task LoadInventoryAsync()
        {
            try
            {
                IsBusy = true;
                _allItems = await _inventoryService.GetInventoryAsync();
                FilterItems();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading inventory");
                if(_dialogService != null) await _dialogService.ShowAlertAsync("Error", "Failed to load inventory items.");
            }
             finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterItems();
        }

        private void FilterItems()
        {
            InventoryItems.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchQuery) 
                ? _allItems 
                : _allItems.Where(i => i.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            foreach (var item in filtered)
            {
                InventoryItems.Add(item);
            }
        }

        [RelayCommand]
        public async Task RefreshInventory()
        {
            await LoadInventoryAsync();
        }
        [ObservableProperty]
        private bool _isDetailVisible;

        [ObservableProperty]
        private InventoryItem? _selectedInventoryItem;

        [ObservableProperty]
        private InventoryDetailViewModel? _detailViewModel;

        // Factory/Service Locator needed? Or direct instantiation?
        // Direct instantiation is easier for now, assuming DI for dependencies.
        // But we need IDialogService and IInventoryService to pass.
        // We have them in constructor!
        
        [RelayCommand]
        public void AddInventoryItem()
        {
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = new InventoryDetailViewModel(_inventoryService, _dialogService);
            DetailViewModel.Load(null, categories); // Add Mode with Categories
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadInventoryAsync();
            };
            IsDetailVisible = true;
        }

        [RelayCommand]
        public void EditInventoryItem(InventoryItem item)
        {
            if (item == null) return;
            var categories = _allItems.Select(i => i.Category).Distinct().OrderBy(c => c).ToList();

            DetailViewModel = new InventoryDetailViewModel(_inventoryService, _dialogService);
            DetailViewModel.Load(item, categories); // Edit Mode with Categories
            DetailViewModel.CloseRequested += (s, e) => IsDetailVisible = false;
            DetailViewModel.ItemSaved += (s, e) => 
            {
                IsDetailVisible = false;
                _ = LoadInventoryAsync();
            };
            IsDetailVisible = true;
        }

        [RelayCommand]
        public async Task DeleteInventoryItem(InventoryItem item)
        {
             if (item == null) return;
             // Stub for Delete logic
             await _dialogService.ShowAlertAsync("Coming Soon", "Delete Inventory feature is under construction.");
        }
    }
}
