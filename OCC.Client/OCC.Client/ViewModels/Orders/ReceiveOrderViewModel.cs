using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Orders
{
    public partial class ReceiveOrderViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ReceiveOrderViewModel> _logger;

        [ObservableProperty]
        private Order _order = new();

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<ReceiveLineItem> AvailableLines { get; } = new();

        public event EventHandler? CloseRequested;
        public event EventHandler? ReceiptProccessed;

        public ReceiveOrderViewModel(
            IOrderService orderService, 
            IDialogService dialogService,
            ILogger<ReceiveOrderViewModel> logger)
        {
            _orderService = orderService;
            _dialogService = dialogService;
            _logger = logger;
        }

        public ReceiveOrderViewModel()
        {
            // Design time
            _orderService = null!;
            _dialogService = null!;
            _logger = null!;
            _order = new Order 
            { 
                OrderNumber = "PO-2401-001", 
                SupplierName = "Demo Supplier",
                OrderDate = DateTime.Now
            };
        }

        public void Initialize(Order order)
        {
            Order = order;
            AvailableLines.Clear();
            foreach(var line in order.Lines)
            {
                // Only add lines that aren't fully received yet (optional, or show all for correction?)
                // Lets show all so they can see history, but disable if complete?
                // For now, show all.
                AvailableLines.Add(new ReceiveLineItem(line));
            }
        }

        [RelayCommand]
        public async Task SubmitReceipt()
        {
            try
            {
                var linesToProcess = AvailableLines.Where(l => l.ReceiveNow > 0).ToList();

                if (!linesToProcess.Any())
                {
                    await _dialogService.ShowAlertAsync("Validation", "Please enter a quantity to receive for at least one item.");
                    return;
                }

                // Check over-receiving?
                // Warn if receiving more than ordered?
                // For now allow it (over-delivery happens).

                IsBusy = true;

                // Prepare Payload
                var updatedLines = linesToProcess.Select(wrapper => 
                {
                    var copy = new OrderLine 
                    {
                        Id = wrapper.Line.Id,
                        QuantityReceived = wrapper.Line.QuantityReceived + wrapper.ReceiveNow
                    };
                    return copy;
                }).ToList();

                await _orderService.ReceiveOrderAsync(Order, updatedLines);
                
                await _dialogService.ShowAlertAsync("Success", "Inventories updated and receipt processed.");
                
                ReceiptProccessed?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing receipt for order {OrderNumber}", Order?.OrderNumber);
                await _dialogService.ShowAlertAsync("Error", $"Failed to process receipt: {ex.Message}");
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
        
        [RelayCommand]
        public void ReceiveAll()
        {
            foreach(var line in AvailableLines)
            {
                if (line.Remaining > 0)
                    line.ReceiveNow = line.Remaining;
            }
        }
    }

    public partial class ReceiveLineItem : ObservableObject
    {
        public OrderLine Line { get; }
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NewTotalReceived))]
        [NotifyPropertyChangedFor(nameof(NewRemaining))]
        private double _receiveNow;

        public ReceiveLineItem(OrderLine line)
        {
            Line = line;
        }

        public double Remaining => Math.Max(0, Line.QuantityOrdered - Line.QuantityReceived);
        
        public double NewTotalReceived => Line.QuantityReceived + ReceiveNow;
        public double NewRemaining => Math.Max(0, Line.QuantityOrdered - NewTotalReceived);
    }
}
