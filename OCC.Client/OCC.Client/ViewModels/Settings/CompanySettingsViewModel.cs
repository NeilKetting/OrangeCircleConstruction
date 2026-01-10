using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System.Threading.Tasks;
using System;
using OCC.Client.ViewModels.Core;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Settings
{
    public partial class CompanySettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private CompanyDetails _companyDetails = new();

        [ObservableProperty]
        private bool _isBusy;

        private readonly ILogger<CompanySettingsViewModel> _logger;

        public CompanySettingsViewModel(ISettingsService settingsService, IDialogService dialogService, ILogger<CompanySettingsViewModel> logger)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _logger = logger;
            LoadData();
        }
        
        public CompanySettingsViewModel() 
        { 
             // Design time
             _settingsService = null!;
             _dialogService = null!;
             _logger = null!;
             CompanyDetails = new CompanyDetails();
        }

        public async void LoadData()
        {
            try
            {
                IsBusy = true;
                CompanyDetails = await _settingsService.GetCompanyDetailsAsync();
                
                Departments.Clear();
                if (CompanyDetails.DepartmentEmails != null)
                {
                    foreach(var d in CompanyDetails.DepartmentEmails) Departments.Add(d);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
                if(_dialogService != null) 
                    await _dialogService.ShowAlertAsync("Error", $"Failed to load settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<DepartmentEmail> _departments = new();

        [RelayCommand]
        public void AddDepartment()
        {
            Departments.Add(new DepartmentEmail { Department = "New Dept", EmailAddress = "" });
        }

        [RelayCommand]
        public void RemoveDepartment(DepartmentEmail dept)
        {
            if (Departments.Contains(dept))
            {
                Departments.Remove(dept);
            }
        }

        [RelayCommand]
        public async Task Save()
        {
            try
            {
                IsBusy = true;
                
                // Sync back
                CompanyDetails.DepartmentEmails = new System.Collections.Generic.List<DepartmentEmail>(Departments);

                await _settingsService.SaveCompanyDetailsAsync(CompanyDetails);
                
                await _dialogService.ShowAlertAsync("Success", "Company details saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
                await _dialogService.ShowAlertAsync("Error", $"Failed to save settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
