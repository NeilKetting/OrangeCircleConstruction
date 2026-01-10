using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Orders
{
    public partial class CreateOrderViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly ISupplierService _supplierService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IDialogService _dialogService;
        private readonly ILogger<CreateOrderViewModel> _logger;

        [ObservableProperty]
        private Order _newOrder = new();

        [ObservableProperty]
        private OrderLine _newLine = new();
        
        // Selections
        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<ProjectBase> Projects { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();
        
        // Selected Items
        [ObservableProperty]
        private Supplier? _selectedSupplier;
        
        [ObservableProperty]
        private Customer? _selectedCustomer;
        
        [ObservableProperty]
        private InventoryItem? _selectedInventoryItem;

        // UI Helpers
        [ObservableProperty]
        private bool _isPurchaseOrder;
        
        [ObservableProperty]
        private bool _isSalesOrder;
        
        [ObservableProperty]
        private bool _isReturnOrder;

        public event EventHandler? CloseRequested;


        public CreateOrderViewModel(
            IOrderService orderService, 
            IInventoryService inventoryService,
            ISupplierService supplierService,
            IRepository<Customer> customerRepository,
            IRepository<Project> projectRepository, 
            IDialogService dialogService,
            ILogger<CreateOrderViewModel> logger)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _supplierService = supplierService;
            _customerRepository = customerRepository;
            _projectRepository = projectRepository;
            _dialogService = dialogService;
            _logger = logger;
            
            InitializeOrder();
        }

        public CreateOrderViewModel() 
        {
            _orderService = null!;
            _inventoryService = null!;
            _supplierService = null!;
            _customerRepository = null!;
            _projectRepository = null!;
            _dialogService = null!;
            _logger = null!;
        } // Design-time support

        public async void LoadData()
        {
            try
            {
                // Parallel load
                var t1 = LoadSuppliers();
                var t2 = LoadCustomers();
                var t3 = LoadProjects();
                var t4 = LoadInventory();
                
                await Task.WhenAll(t1, t2, t3, t4);
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "Error loading order data");
                 await _dialogService.ShowAlertAsync("Error", "Failed to load order data. Please try again.");
            }
        }

        private void InitializeOrder()
        {
            NewOrder = new Order
            {
                OrderDate = DateTime.Now,
                OrderNumber = $"PO-{DateTime.Now:yyMM}-{new Random().Next(1000, 9999)}",
                OrderType = OrderType.PurchaseOrder, // Default
                TaxRate = 0.15m
            };
            UpdateOrderTypeFlags();
        }

        partial void OnNewOrderChanged(Order value)
        {
            UpdateOrderTypeFlags();
        }
        
        // Watch for OrderType changes (needs a way to trigger, usually implicit via UI binding to property)
        // Since Order is a nested object, we might need a wrapper or manual trigger.
        // Assuming UI binds to NewOrder.OrderType directly.
        
        [RelayCommand]
        public void ChangeOrderType(OrderType type)
        {
             NewOrder.OrderType = type;
             
             // Update Number Prefix
             string prefix = type == OrderType.PurchaseOrder ? "PO" : type == OrderType.SalesOrder ? "SO" : "RET";
             NewOrder.OrderNumber = $"{prefix}-{DateTime.Now:yyMM}-{new Random().Next(1000, 9999)}";
             
             UpdateOrderTypeFlags();
             OnPropertyChanged(nameof(NewOrder));
        }

        private void UpdateOrderTypeFlags()
        {
            IsPurchaseOrder = NewOrder.OrderType == OrderType.PurchaseOrder;
            IsSalesOrder = NewOrder.OrderType == OrderType.SalesOrder;
            IsReturnOrder = NewOrder.OrderType == OrderType.ReturnToInventory;
        }

        private async Task LoadSuppliers()
        {
             var list = await _supplierService.GetSuppliersAsync();
             Suppliers.Clear();
             foreach(var i in list) Suppliers.Add(i);
        }
        
        private async Task LoadCustomers()
        {
             var list = await _customerRepository.GetAllAsync();
             Customers.Clear();
             foreach(var i in list) Customers.Add(i);
        }
        
        private async Task LoadProjects()
        {
             var list = await _projectRepository.GetAllAsync();
             Projects.Clear();
             foreach(var i in list) Projects.Add(new ProjectBase { Id = i.Id, Name = i.Name });
        }
        
        private async Task LoadInventory()
        {
             var list = await _inventoryService.GetInventoryAsync();
             InventoryItems.Clear();
             foreach(var i in list) InventoryItems.Add(i);
        }

        partial void OnSelectedSupplierChanged(Supplier? value)
        {
            if (value != null)
            {
                NewOrder.SupplierId = value.Id;
                NewOrder.SupplierName = value.Name;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                NewOrder.EntityVatNo = value.VatNumber;
            }
        }

        partial void OnSelectedCustomerChanged(Customer? value)
        {
            if (value != null)
            {
                NewOrder.CustomerId = value.Id;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                // Customer might not have VAT logic in same way, but reuse fields
            }
        }
        
        partial void OnSelectedInventoryItemChanged(InventoryItem? value)
        {
            if (value != null)
            {
                NewLine.InventoryItemId = value.Id;
                NewLine.Description = value.ProductName;
                NewLine.ItemCode = value.ProductName; // Or Id?
                NewLine.UnitOfMeasure = value.UnitOfMeasure;
                // NewLine.UnitPrice = value.Price; // InventoryItem needs Price? Not in model yet.
            }
        }

        [RelayCommand]
        public void AddLine()
        {
            if (string.IsNullOrWhiteSpace(NewLine.Description) && SelectedInventoryItem == null) return;

            // Calculate Checks
            NewLine.CalculateTotal(NewOrder.TaxRate);

            var line = new OrderLine
            {
                InventoryItemId = NewLine.InventoryItemId,
                ItemCode = NewLine.ItemCode,
                Description = NewLine.Description,
                QuantityOrdered = NewLine.QuantityOrdered,
                QuantityReceived = NewLine.QuantityReceived, // For Returns/Immediate
                UnitOfMeasure = NewLine.UnitOfMeasure,
                UnitPrice = NewLine.UnitPrice,
                VatAmount = NewLine.VatAmount,
                LineTotal = NewLine.LineTotal
            };
            
            NewOrder.Lines.Add(line);
            
            // Trigger property change for Totals on Order?
            // Order Model handles Sums via computed properties, so we just need to notify UI
            OnPropertyChanged(nameof(NewOrder));

            // Reset Line
            NewLine = new OrderLine();
            SelectedInventoryItem = null;
        }
        
        [RelayCommand]
        public void RemoveLine(OrderLine line)
        {
            if (line == null) return;
            NewOrder.Lines.Remove(line);
            OnPropertyChanged(nameof(NewOrder));
        }

        [RelayCommand]
        public async Task SubmitOrder()
        {
            try
            {
                if (NewOrder.Lines.Count == 0) 
                {
                    await _dialogService.ShowAlertAsync("Validation", "Please add at least one item.");
                    return;
                }

                await _orderService.CreateOrderAsync(NewOrder);
                
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting order: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error", "Failed to submit order.");
            }
        }
        
        [RelayCommand]
        public void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ProjectBase 
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }
}
