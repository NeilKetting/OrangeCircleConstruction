using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Messages;

namespace OCC.Client.ViewModels.Time
{
    public partial class TimeViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private DateTimeOffset? _weekStarting;

        [ObservableProperty]
        private string _selectedUserEmail = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _availableUsers = new();

        [ObservableProperty]
        private ObservableCollection<TimesheetRowViewModel> _rows = new();

        // Daily Totals
        [ObservableProperty] private double _mondayTotal;
        [ObservableProperty] private double _tuesdayTotal;
        [ObservableProperty] private double _wednesdayTotal;
        [ObservableProperty] private double _thursdayTotal;
        [ObservableProperty] private double _fridayTotal;
        [ObservableProperty] private double _saturdayTotal;
        [ObservableProperty] private double _sundayTotal;
        [ObservableProperty] private double _grandTotal;

        [ObservableProperty]
        private string _lastUpdatedText = "Updated on " + DateTime.Now.ToString("dd MMMM yyyy HH:mmtt").ToLower();

        public TimeViewModel(ITimeService timeService, IAuthService authService)
        {
            _timeService = timeService;
            _authService = authService;
            
            // Default to start of current week
            var diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
            WeekStarting = new DateTimeOffset(DateTime.Today.AddDays(-1 * diff).Date);

            InitializeCommand.Execute(null);
        }

        [RelayCommand]
        private async Task Initialize()
        {
            // Mock users for the dropdown
            AvailableUsers.Add("origize63@gmail.com");
            AvailableUsers.Add("admin@occ.co.za");
            SelectedUserEmail = AvailableUsers.FirstOrDefault() ?? string.Empty;

            await LoadWeeklyData();
        }

        private async Task LoadWeeklyData()
        {
            Rows.Clear();
            // Add some mock rows for now
            Rows.Add(new TimesheetRowViewModel 
            { 
                ProjectName = "All other tasks", 
                TaskName = "Test",
                FridayHours = 8
            });

            foreach (var row in Rows)
            {
                row.PropertyChanged += (s, e) => CalculateTotals();
            }

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            MondayTotal = Rows.Sum(r => r.MondayHours ?? 0);
            TuesdayTotal = Rows.Sum(r => r.TuesdayHours ?? 0);
            WednesdayTotal = Rows.Sum(r => r.WednesdayHours ?? 0);
            ThursdayTotal = Rows.Sum(r => r.ThursdayHours ?? 0);
            FridayTotal = Rows.Sum(r => r.FridayHours ?? 0);
            SaturdayTotal = Rows.Sum(r => r.SaturdayHours ?? 0);
            SundayTotal = Rows.Sum(r => r.SundayHours ?? 0);
            GrandTotal = MondayTotal + TuesdayTotal + WednesdayTotal + ThursdayTotal + FridayTotal + SaturdayTotal + SundayTotal;
        }

        [RelayCommand]
        private void PreviousWeek()
        {
            WeekStarting = WeekStarting?.AddDays(-7);
        }

        [RelayCommand]
        private void NextWeek()
        {
            WeekStarting = WeekStarting?.AddDays(7);
        }

        [RelayCommand]
        private void OpenRollCall()
        {
            WeakReferenceMessenger.Default.Send(new SwitchTabMessage("RollCall"));
        }

        partial void OnWeekStartingChanged(DateTimeOffset? value)
        {
            _ = LoadWeeklyData();
        }
    }
}
