using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
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
        private readonly IPdfService _pdfService;

        [ObservableProperty]
        private Order _newOrder = new();

        [ObservableProperty]
        private bool _isReadOnly;

        // Proxy Properties for Order Totals (Model doesn't implement INPC)
        public decimal OrderSubTotal => NewOrder?.SubTotal ?? 0;
        public decimal OrderVat => NewOrder?.VatTotal ?? 0;
        public decimal OrderTotal => NewOrder?.TotalAmount ?? 0;

        [ObservableProperty]
        private OrderLine _newLine = new();
        
        // Selections
        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<ProjectBase> Projects { get; } = new();
        public ObservableCollection<InventoryItem> InventoryItems { get; } = new();
        
        public List<string> AvailableUOMs { get; } = new() { "ea", "m", "kg", "L", "m2", "m3", "box", "roll", "pack" };
        
        // Selected Items
        [ObservableProperty]
        [property: CustomValidation(typeof(CreateOrderViewModel), nameof(ValidateSupplierSelection))]
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

        // Validation Test Proxy



        [ObservableProperty]
        private bool _isAddingNewProduct;
        
        [ObservableProperty]
        private string _newProductName = string.Empty;

        [ObservableProperty]
        private string _newProductUOM = "ea";
        
        [ObservableProperty]
        private bool _isAddingNewSupplier;
        
        [ObservableProperty]
        private string _newSupplierName = string.Empty;

        // --- Logic ---
        
        [ObservableProperty]
        private bool _isOfficeDelivery = true; // Default
        
        [ObservableProperty]
        private bool _isSiteDelivery;
        
        [ObservableProperty]
        [property: CustomValidation(typeof(CreateOrderViewModel), nameof(ValidateProjectSelection))]
        private ProjectBase? _selectedProject;

        // Validation Logic
        public static ValidationResult? ValidateSupplierSelection(Supplier? supplier, ValidationContext context)
        {
            var vm = (CreateOrderViewModel)context.ObjectInstance;
            if (vm.IsPurchaseOrder && supplier == null)
            {
                return new ValidationResult("Supplier is required for Purchase Orders.");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateProjectSelection(ProjectBase? project, ValidationContext context)
        {
            var vm = (CreateOrderViewModel)context.ObjectInstance;
            if (vm.IsSalesOrder && project == null)
            {
                return new ValidationResult("Project is required for Sales Orders.");
            }
            return ValidationResult.Success;
        }

        public event EventHandler? CloseRequested;

        public CreateOrderViewModel(
            IOrderService orderService, 
            IInventoryService inventoryService,
            ISupplierService supplierService,
            IRepository<Customer> customerRepository,
            IRepository<Project> projectRepository, 
            IDialogService dialogService,
            ILogger<CreateOrderViewModel> logger,
            IPdfService pdfService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
            _supplierService = supplierService;
            _customerRepository = customerRepository;
            _projectRepository = projectRepository;
            _dialogService = dialogService;
            _logger = logger;
            _pdfService = pdfService;
            
            Reset();
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
            _pdfService = null!;
            NewOrder.OrderNumber = "DEMO-0000";
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

        public void Reset()
        {
            NewOrder = new Order
            {
                OrderDate = DateTime.Now,
                OrderNumber = $"PO-{DateTime.Now:yyMM}-{new Random().Next(1000, 9999)}",
                OrderType = OrderType.PurchaseOrder, // Default
                TaxRate = 0.15m,
                DestinationType = OrderDestinationType.Stock
            };
            IsOfficeDelivery = true;
            NewOrder.Attention = null; // Ensure null for validation test
            UpdateOrderTypeFlags();
        }

        public void LoadExistingOrder(Order order)
        {
            // Set the order
            NewOrder = order;
            
            // Map Selections
            if (Suppliers.Any()) 
                SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == order.SupplierId);
                
            if (Customers.Any())
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == order.CustomerId);
                
            if (Projects.Any())
                SelectedProject = Projects.FirstOrDefault(p => p.Id == order.ProjectId);
                
            // Update Flags
            UpdateOrderTypeFlags();
            
            // Delivery Flags
            if (order.DestinationType == OrderDestinationType.Site) IsSiteDelivery = true;
            else IsOfficeDelivery = true;
            
            OnPropertyChanged(nameof(NewOrder));
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
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

        partial void OnIsOfficeDeliveryChanged(bool value)
        {
            if (value)
            {
                NewOrder.DestinationType = OrderDestinationType.Stock;
                IsSiteDelivery = false;
                // Maybe reset address to default office address here if needed
                // NewOrder.EntityAddress = "Office Address...";
            }
        }

        partial void OnIsSiteDeliveryChanged(bool value)
        {
            if (value)
            {
                NewOrder.DestinationType = OrderDestinationType.Site;
                IsOfficeDelivery = false;
                // If Project already selected, update address
                if (SelectedProject != null)
                {
                    NewOrder.EntityAddress = SelectedProject.Location;
                }
            }
        }
        
        partial void OnSelectedProjectChanged(ProjectBase? value)
        {
             if (value != null)
             {
                 NewOrder.ProjectId = value.Id;
                 NewOrder.ProjectName = value.Name;

                 // If Sales Order, this is the "Customer"
                 if (IsSalesOrder)
                 {
                     NewOrder.CustomerId = value.Id; // Using Project ID as Customer ID for tracking
                     NewOrder.EntityAddress = value.Location;
                 }
                 // If Purchase Order AND Site Delivery, this is destination
                 else if (IsPurchaseOrder && IsSiteDelivery)
                 {
                     NewOrder.EntityAddress = value.Location;
                 }
                 // If Return Order, this is source
                 else if (IsReturnOrder)
                 {
                     // Typically source address logic would go here
                 }
                 OnPropertyChanged(nameof(NewOrder)); // Notify NewOrder changes (Address etc)
             }
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
             foreach(var i in list) Projects.Add(new ProjectBase { Id = i.Id, Name = i.Name, Location = i.Location });
        }
        
        private async Task LoadInventory()
        {
             var list = await _inventoryService.GetInventoryAsync();
             InventoryItems.Clear();
             foreach(var i in list) InventoryItems.Add(i);
        }

        [ObservableProperty]
        private bool _shouldPrintOrder;

        [ObservableProperty]
        private bool _shouldEmailOrder;

        partial void OnSelectedSupplierChanged(Supplier? value)
        {
            if (value != null)
            {

                NewOrder.SupplierId = value.Id;
                NewOrder.SupplierName = value.Name;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                NewOrder.EntityVatNo = value.VatNumber;
                NewOrder.Attention = value.ContactPerson; 
                OnPropertyChanged(nameof(NewOrder)); // Notify NewOrder changes
            }
        }

        partial void OnSelectedCustomerChanged(Customer? value)
        {
            if (value != null)
            {
                NewOrder.CustomerId = value.Id;
                NewOrder.EntityAddress = value.Address;
                NewOrder.EntityTel = value.Phone;
                OnPropertyChanged(nameof(NewOrder)); // Notify NewOrder changes
            }
        }
        
        partial void OnSelectedInventoryItemChanged(InventoryItem? value)
        {
            if (value != null)
            {
                // Create a NEW instance to ensure UI updates all properties (like UOM)
                NewLine = new OrderLine
                {
                    InventoryItemId = value.Id,
                    Description = value.ProductName,
                    ItemCode = value.ProductName,
                    UnitOfMeasure = value.UnitOfMeasure,
                    UnitPrice = value.AverageCost // Prefill with Avg Cost
                };
            }
        }

        [RelayCommand]
        public async Task AddLine()
        {
            if (string.IsNullOrWhiteSpace(NewLine.Description) && SelectedInventoryItem == null) return;

            // Calculate Checks
            NewLine.CalculateTotal(NewOrder.TaxRate);
            
            // Debugging: Log what we are trying to add
            System.Diagnostics.Debug.WriteLine($"AddLine: Qty={NewLine.QuantityOrdered}, Price={NewLine.UnitPrice}, Total={NewLine.LineTotal}");

            var line = new OrderLine
            {
                InventoryItemId = NewLine.InventoryItemId,
                ItemCode = NewLine.ItemCode,
                Description = NewLine.Description,
                QuantityOrdered = NewLine.QuantityOrdered,
                QuantityReceived = NewLine.QuantityReceived,
                UnitOfMeasure = NewLine.UnitOfMeasure,
                UnitPrice = NewLine.UnitPrice,
                VatAmount = NewLine.VatAmount,
                LineTotal = NewLine.LineTotal
            };
            
            // Double check total - if R0 but we have qty/price, force logic
            if (line.LineTotal == 0 && line.QuantityOrdered > 0 && line.UnitPrice > 0)
            {
                 line.CalculateTotal(NewOrder.TaxRate);
            }

            // Price Check
            if (SelectedInventoryItem != null && SelectedInventoryItem.AverageCost > 0)
            {
                decimal threshold = SelectedInventoryItem.AverageCost * 1.10m;
                if (line.UnitPrice > threshold)
                {
                    decimal pctIncrease = ((line.UnitPrice - SelectedInventoryItem.AverageCost) / SelectedInventoryItem.AverageCost) * 100m;
                    bool confirm = await _dialogService.ShowConfirmationAsync(
                        "Price Increase Warning", 
                        $"The price of R{line.UnitPrice:F2} is {pctIncrease:F0}% higher than the average cost (R{SelectedInventoryItem.AverageCost:F2}).\n\nDo you want to accept this price and update the average cost?");
                    
                    if (confirm)
                    {
                        // Update Avg Cost immediately to suppress future warnings for this price
                        SelectedInventoryItem.AverageCost = line.UnitPrice;
                        try 
                        {
                            await _inventoryService.UpdateItemAsync(SelectedInventoryItem);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "Failed to update avg cost");
                        }
                    }
                }
            }

            NewOrder.Lines.Add(line);
            
            // Trigger property change for Totals on Order
            OnPropertyChanged(nameof(NewOrder));
            
            // Notify Totals
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));

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
            
            // Notify Totals
            OnPropertyChanged(nameof(OrderSubTotal));
            OnPropertyChanged(nameof(OrderVat));
            OnPropertyChanged(nameof(OrderTotal));
        }
        
        // --- Quick Add Commands ---
        
        [RelayCommand]
        public void ToggleQuickAddProduct()
        {
            IsAddingNewProduct = !IsAddingNewProduct;
            if (IsAddingNewProduct)
            {
                NewProductName = "";
                NewProductUOM = "ea";
            }
        }
        
        [RelayCommand]
        public void ToggleQuickAddSupplier()
        {
            IsAddingNewSupplier = !IsAddingNewSupplier;
            if (IsAddingNewSupplier)
            {
                NewSupplierName = "";
            }
        }

        [RelayCommand]
        public async Task QuickCreateProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProductName)) return;
            
            try
            {
                var item = new InventoryItem 
                { 
                    ProductName = NewProductName, 
                    UnitOfMeasure = NewProductUOM 
                };
                
                var created = await _inventoryService.CreateItemAsync(item);
                
                InventoryItems.Add(created);
                SelectedInventoryItem = created;
                IsAddingNewProduct = false;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to quick create product");
                await _dialogService.ShowAlertAsync("Error", "Failed to create product.");
            }
        }
        
        [RelayCommand]
        public async Task QuickCreateSupplier()
        {
            if (string.IsNullOrWhiteSpace(NewSupplierName)) return;
            
            try
            {
                 var supplier = new Supplier { Name = NewSupplierName };
                 var created = await _supplierService.CreateSupplierAsync(supplier);
                 
                 // If the service returns the object (which it should, checking interface return type)
                 // NOTE: Interface says Task<Supplier>, so yes.
                 
                 Suppliers.Add(created);
                 SelectedSupplier = created;
                 IsAddingNewSupplier = false;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to quick create supplier");
                await _dialogService.ShowAlertAsync("Error", "Failed to create supplier.");
            }
        }

        [RelayCommand]
        public async Task SubmitOrder()
        {
            try
            {
                // Force validation on key selection properties
                ValidateProperty(SelectedSupplier, nameof(SelectedSupplier));
                ValidateProperty(SelectedProject, nameof(SelectedProject));

                ValidateAllProperties();

                if (HasErrors)
                {
                    await _dialogService.ShowAlertAsync("Validation", "Please correct the errors before submitting.");
                    return;
                }

                if (NewOrder.Lines.Count == 0) 
                {
                    await _dialogService.ShowAlertAsync("Validation", "Please add at least one item.");
                    return;
                }

                var createdOrder = await _orderService.CreateOrderAsync(NewOrder);
                
                if (ShouldPrintOrder)
                {
                    try
                    {
                        var path = await _pdfService.GenerateOrderPdfAsync(createdOrder);
                        
                        // Open PDF
                        new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo(path)
                            {
                                UseShellExecute = true
                            }
                        }.Start();
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "Failed to print order");
                        await _dialogService.ShowAlertAsync("Warning", "Order created, but failed to generate PDF.");
                    }
                }

                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting order: {ex.Message}");
                // Show detailed error
                await _dialogService.ShowAlertAsync("Error", $"Failed to submit order: {ex.Message}");
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
        public string Location { get; set; } = "";
        
        public override string ToString() => Name;
    }
}
