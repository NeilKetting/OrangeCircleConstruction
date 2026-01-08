using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class HistoryViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;
        private readonly IExportService _exportService;

        #region Filter Properties

        [ObservableProperty]
        private string _selectedRange = "This Week";

        public ObservableCollection<string> RangeOptions { get; } = new ObservableCollection<string>
        {
            "Today",
            "Yesterday",
            "This Week",
            "Last Week",
            "This Month",
            "Last Month",
            "Custom"
        };

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTime.Today;
        
        [ObservableProperty]
        private bool _isCustomDateEnabled;

        #endregion

        #region Data Properties

        [ObservableProperty]
        private ObservableCollection<HistoryRecordViewModel> _records = new();

        [ObservableProperty]
        private decimal _totalWages;

        [ObservableProperty]
        private double _totalHours;

        [ObservableProperty]
        private bool _isLoading;

        #endregion

        public HistoryViewModel(ITimeService timeService, IExportService exportService)
        {
            _timeService = timeService;
            _exportService = exportService;
            
            // Set default range logic (This Week)
            SetRange("This Week");
            
            // Initial Load
            InitializeCommand.Execute(null);
        }

        [RelayCommand]
        private async Task ExportCsv()
        {
            if (Records == null || !Records.Any()) return;
            
            var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), $"Attendance_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            
            // Create a projection for clean CSV
            var data = Records.Select(r => new 
            {
                Date = r.Date.ToShortDateString(),
                Employee = r.EmployeeName,
                Branch = r.Branch,
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Wage = r.WageDisplay
            });

            await _exportService.ExportToCsvAsync(data, path);
            await _exportService.OpenFileAsync(path); // Open directly? Maybe users prefers folder. 
            // For now, let's open it so they see it.
        }

        [RelayCommand]
        private async Task PrintReport()
        {
            if (Records == null || !Records.Any()) return;

            var columns = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Date", "Date" },
                { "Employee", "Employee" },
                { "Branch", "Branch" },
                { "In", "In" },
                { "Out", "Out" },
                { "Status", "Status" },
                { "Hours", "Hours" },
                { "Wage", "Wage Cost" }
            };

            var data = Records.Select(r => new 
            {
                Date = r.Date.ToShortDateString(),
                Employee = r.EmployeeName,
                Branch = r.Branch,
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Wage = r.WageDisplay
            });
            
            var title = $"Attendance Report: {StartDate:dd MMM} - {EndDate:dd MMM yyyy}";
            var path = await _exportService.GenerateHtmlReportAsync(data, title, columns);
            
            await _exportService.OpenFileAsync(path);
        }

        [RelayCommand]
        private async Task Initialize()
        {
            await LoadData();
        }

        async partial void OnSelectedRangeChanged(string value)
        {
            SetRange(value);
            await LoadData();
        }

        private void SetRange(string range)
        {
            var today = DateTime.Today;
            IsCustomDateEnabled = false;

            switch (range)
            {
                case "Today":
                    StartDate = today;
                    EndDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "Yesterday":
                    StartDate = today.AddDays(-1);
                    EndDate = today.AddDays(-1).AddDays(1).AddTicks(-1);
                    break;
                case "This Week":
                    // Assuming Week starts Monday
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    StartDate = today.AddDays(-1 * diff).Date;
                    EndDate = today.AddDays(1).AddTicks(-1); // Up to now
                    break;
                case "Last Week":
                    var lastWeek = today.AddDays(-7);
                    int lwDiff = (7 + (lastWeek.DayOfWeek - DayOfWeek.Monday)) % 7;
                    StartDate = lastWeek.AddDays(-1 * lwDiff).Date;
                    EndDate = StartDate.AddDays(7).AddTicks(-1); // Full week
                    break;
                case "This Month":
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "Last Month":
                    var lastMonth = today.AddMonths(-1);
                    StartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    EndDate = new DateTime(today.Year, today.Month, 1).AddTicks(-1);
                    break;
                case "Custom":
                    IsCustomDateEnabled = true;
                    // Keep current dates
                    break;
            }
        }
        
        [RelayCommand]
        private async Task Refresh()
        {
            await LoadData();
        }

        // Trigger load when dates change IF Custom is selected
        async partial void OnStartDateChanged(DateTimeOffset value)
        {
            if (SelectedRange == "Custom") await LoadData();
        }

        async partial void OnEndDateChanged(DateTimeOffset value)
        {
            if (SelectedRange == "Custom") await LoadData();
        }

        private async Task LoadData()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var s = StartDate.DateTime;
                var e = EndDate.DateTime;
                
                // Fetch
                var attendance = await _timeService.GetAttendanceByRangeAsync(s, e);
                var allEmployee = await _timeService.GetAllStaffAsync();

                var list = new ObservableCollection<HistoryRecordViewModel>();
                decimal wages = 0;
                double hours = 0;

                foreach (var rec in attendance.OrderByDescending(x => x.Date).ThenBy(x => x.CheckInTime))
                {
                    var emp = allEmployee.FirstOrDefault(em => em.Id == rec.EmployeeId);
                    if (emp == null) continue;

                    var vm = new HistoryRecordViewModel(rec, emp);
                    list.Add(vm);

                    wages += vm.Wage;
                    hours += vm.HoursWorked;
                }

                Records = list;
                TotalWages = wages;
                TotalHours = hours;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
