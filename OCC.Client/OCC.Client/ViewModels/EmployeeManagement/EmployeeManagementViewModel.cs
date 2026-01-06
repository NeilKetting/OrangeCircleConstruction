using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeManagementViewModel : ViewModelBase
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
        private string _activeTab = "Manage Staff";

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

        #endregion

        #region Constructors

        public EmployeeManagementViewModel()
        {
            // Designer constructor
        }

        public EmployeeManagementViewModel(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
            LoadData();
        }

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
        private void SwitchTab(string tabName)
        {
            ActiveTab = tabName;
        }

        #endregion

        #region Methods

        public async void LoadData()
        {
            try 
            {
                var employees = await _employeeRepository.GetAllAsync();
                
                _allEmployees = employees.ToList(); // Cache full list
                FilterEmployees();
                
                TotalStaff = _allEmployees.Count;
                TotalCount = _allEmployees.Count;
                PermanentCount = _allEmployees.Count(s => s.EmploymentType == EmploymentType.Permanent);
                ContractCount = _allEmployees.Count(s => s.EmploymentType == EmploymentType.Contract);
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
            filtered = _selectedFilterIndex switch
            {
                1 => filtered.Where(s => s.EmploymentType == EmploymentType.Permanent),
                2 => filtered.Where(s => s.EmploymentType == EmploymentType.Contract),
                _ => filtered
            };

            Employees = new ObservableCollection<Employee>(filtered);
        }

        #endregion
    }
}
