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

namespace OCC.Client.ViewModels.Orders
{
    public partial class CreateOrderViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IRepository<Project> _projectRepository;

        [ObservableProperty]
        private Order _newOrder = new();

        [ObservableProperty]
        private OrderLine _newLine = new();

        public ObservableCollection<string> Suppliers { get; } = new() { "Builders Warehouse", "Timber City", "Plumb It", "Voltex", "Cashbuild" };
        public ObservableCollection<ProjectBase> Projects { get; } = new();

        public CreateOrderViewModel(IOrderService orderService, IRepository<Project> projectRepository)
        {
            _orderService = orderService;
            _projectRepository = projectRepository;
            
            InitializeOrder();
            LoadProjects();
        }

        // Default constructor for design-time support if needed, or DI container might need empty one if not configured right (removed for DI enforcement)
        // public CreateOrderViewModel() { } 

        private void InitializeOrder()
        {
            NewOrder = new Order
            {
                OrderDate = DateTime.Now,
                OrderNumber = UseMockOrderNumber() // Ideally this comes from backend logic if sequential
            };
        }

        private string UseMockOrderNumber()
        {
             return $"PO-{DateTime.Now.Year}-{new Random().Next(100, 999)}";
        }

        private async void LoadProjects()
        {
            try 
            {
                 var projects = await _projectRepository.GetAllAsync();
                 Projects.Clear();
                 foreach(var p in projects)
                 {
                     Projects.Add(new ProjectBase { Id = p.Id, Name = p.Name });
                 }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
            }
        }

        [RelayCommand]
        public void AddLine()
        {
            if (string.IsNullOrWhiteSpace(NewLine.Description)) return;

            var line = new OrderLine
            {
                Description = NewLine.Description,
                QuantityOrdered = NewLine.QuantityOrdered,
                UnitOfMeasure = NewLine.UnitOfMeasure
            };
            
            NewOrder.Lines.Add(line);
            
            // Reset Line
            NewLine = new OrderLine();
            OnPropertyChanged(nameof(NewOrder)); 
        }

        [RelayCommand]
        public async Task SubmitOrder()
        {
            try
            {
                if (NewOrder.Lines.Count == 0) return;

                await _orderService.CreateOrderAsync(NewOrder);
                
                // Show Success or Navigate Back
                // Reset form for next order
                InitializeOrder();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting order: {ex.Message}");
            }
        }
    }

    // Helper class for ComboBox binding if full Project object is too heavy
    public class ProjectBase 
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }
}
