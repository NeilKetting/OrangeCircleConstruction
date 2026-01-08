using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection; // Added
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeManagementViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<ViewModels.Messages.EntityUpdatedMessage>
    {
        #region Private Members

        private readonly IRepository<Employee> _employeeRepository;
        
        /// <summary>
        /// Cache for all loaded employees to support filtering without database calls
        /// </summary>
        private List<Employee> _allEmployees = new();

        #endregion

        #region Observables

        [ObservableProperty]
        private string _activeTab = "Employees";

        [ObservableProperty]
        private int _totalStaff = 0;

        [ObservableProperty]
        private int _permanentCount = 0;

        [ObservableProperty]
        private int _contractCount = 0;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private int _selectedFilterIndex = 0;

        [ObservableProperty]
        private bool _isAddEmployeePopupVisible;

        [ObservableProperty]
        private EmployeeDetailViewModel? _addEmployeePopup;

        [ObservableProperty]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _selectedBranchFilterIndex = 0;

        [ObservableProperty]
        private TeamManagementViewModel _teamsVM;
        #endregion

        #region Constructors

        public EmployeeManagementViewModel()
        {
            // Designer constructor
            _employeeRepository = null!;
            _teamsVM = null!;
            _serviceProvider = null!;
        }

        private readonly IServiceProvider _serviceProvider;

        public EmployeeManagementViewModel(
            IRepository<Employee> employeeRepository, 
            TeamManagementViewModel teamsVM,
            IServiceProvider serviceProvider)
        {
            _employeeRepository = employeeRepository;
            _teamsVM = teamsVM;
            _serviceProvider = serviceProvider;
            
            _teamsVM.EditTeamRequested += (s, team) => 
            {
                var vm = _serviceProvider.GetRequiredService<TeamDetailViewModel>();
                vm.Load(team);
                vm.CloseRequested += (s2, e2) => IsAddTeamPopupVisible = false;
                TeamDetailPopup = vm;
                IsAddTeamPopupVisible = true;
            };

            LoadData();
            
            // Register for real-time updates
            CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.RegisterAll(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default, this);
        }

        public void Receive(ViewModels.Messages.EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "Employee")
            {
                // Refresh data on any employee change
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(LoadData);
            }
        }

        private void OnEditTeamRequested(object? sender, Team team)
        {
            // Resolve TeamDetailViewModel via DI or Factory if possible, or create manually if dependencies allow.
            // Since we didn't inject a factory, we might need IServiceProvider or pass dependencies.
            // For now, let's assume we can resolve it or create it.
            // We need: IRepository<Team>, IRepository<TeamMember>, IRepository<Employee>, SignalR
            // This is getting complex to instantiate manually.
            // BETTER: Inject IServiceProvider to resolve transient VMs.
        }
        
        // Simpler for now: Add Properties first.
        [ObservableProperty]
        private bool _isAddTeamPopupVisible;

        [ObservableProperty]
        private TeamDetailViewModel? _teamDetailPopup;

        #endregion

        #region Commands

        [RelayCommand]
        private void AddEmployee()
        {
            AddEmployeePopup = new EmployeeDetailViewModel(_employeeRepository);
            AddEmployeePopup.CloseRequested += (s, e) => IsAddEmployeePopupVisible = false;
            AddEmployeePopup.EmployeeAdded += (s, e) => 
            {
                IsAddEmployeePopupVisible = false;
                LoadData();
            };
            IsAddEmployeePopupVisible = true;
        }

        [RelayCommand]
        public void EditEmployee(Employee employee)
        {
            if (employee == null) return;

            AddEmployeePopup = new EmployeeDetailViewModel(_employeeRepository);
            AddEmployeePopup.Load(employee);
            AddEmployeePopup.CloseRequested += (s, e) => IsAddEmployeePopupVisible = false;
            AddEmployeePopup.EmployeeAdded += (s, e) => 
            {
                IsAddEmployeePopupVisible = false;
                LoadData(); 
            };
            IsAddEmployeePopupVisible = true;
        }

        [RelayCommand]
        public async Task DeleteEmployee(Employee employee)
        {
            if (employee == null) return;

            // Optional: Confirm dialog could go here
            await _employeeRepository.DeleteAsync(employee.Id);
            LoadData();
        }

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
        }

        [RelayCommand]
        private async Task ExportEmployees()
        {
            try
            {
                if (_allEmployees == null || !_allEmployees.Any()) return;

                var options = new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                };
                
                string jsonString = System.Text.Json.JsonSerializer.Serialize(Employees, options);

                string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fileName = $"OCC_Employees_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string fullPath = System.IO.Path.Combine(folder, fileName);

                await System.IO.File.WriteAllTextAsync(fullPath, jsonString);

                // Notify user via toast
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new ViewModels.Messages.UpdateStatusMessage($"Backup Saved to Documents: {fileName}"));
                
                System.Diagnostics.Debug.WriteLine($"Exported to: {fullPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Export failed: {ex.Message}");
            }
        }

        #endregion

        #region Methods

        public async void LoadData()
        {
            try 
            {
                // Capture current selection ID
                var selectedId = SelectedEmployee?.Id;

                var employees = await _employeeRepository.GetAllAsync();
                
                _allEmployees = employees.ToList(); // Cache full list
                FilterEmployees();

                // Restore selection
                if (selectedId.HasValue)
                {
                    SelectedEmployee = Employees.FirstOrDefault(e => e.Id == selectedId.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex.Message}");
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            FilterEmployees();
        }

        partial void OnSelectedFilterIndexChanged(int value)
        {
            FilterEmployees();
        }

        partial void OnSelectedBranchFilterIndexChanged(int value)
        {
            FilterEmployees();
        }

        partial void OnSelectedEmployeeChanged(Employee? value)
        {
            // Selection logic only, double-click handles edit now
        }

        #endregion

        #region Helper Methods

        private void FilterEmployees()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            // 1. Text Search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(s => 
                       (s.FirstName?.ToLower().Contains(query) ?? false) ||
                       (s.LastName?.ToLower().Contains(query) ?? false) ||
                       (s.EmployeeNumber?.ToLower().Contains(query) ?? false)
                );
            }

            // 2. Type Filter
            filtered = SelectedFilterIndex switch
            {
                1 => filtered.Where(s => s.EmploymentType == EmploymentType.Permanent),
                2 => filtered.Where(s => s.EmploymentType == EmploymentType.Contract),
                _ => filtered
            };

            // 3. Branch Filter
            // 0 = All, 1 = JHB, 2 = CPT
            filtered = SelectedBranchFilterIndex switch
            {
                1 => filtered.Where(s => s.Branch == "Johannesburg"),
                2 => filtered.Where(s => s.Branch == "Cape Town"),
                _ => filtered
            };

            var resultList = filtered.ToList();
            Employees = new ObservableCollection<Employee>(resultList);

            // Update Stats based on FILTERED results
            TotalStaff = resultList.Count;
            TotalCount = resultList.Count;
            PermanentCount = resultList.Count(s => s.EmploymentType == EmploymentType.Permanent);
            ContractCount = resultList.Count(s => s.EmploymentType == EmploymentType.Contract);
        }

        #endregion
    }
}
