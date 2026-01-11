using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core; // Added
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OCC.Client.ViewModels.Bugs
{
    public partial class BugListViewModel : ViewModelBase
    {
        private readonly IBugReportService _bugService;
        
        [ObservableProperty]
        private ObservableCollection<BugReport> _bugs = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private BugReport? _selectedBug;

        public BugListViewModel(IBugReportService bugService)
        {
            _bugService = bugService;
            LoadBugsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadBugs()
        {
            IsLoading = true;
            try
            {
                var list = await _bugService.GetBugReportsAsync();
                Bugs = new ObservableCollection<BugReport>(list.OrderByDescending(x => x.ReportedDate));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bugs: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
