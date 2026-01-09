using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;

using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Time
{
    public partial class StaffAttendanceViewModel : ViewModelBase
    {
        #region Private Members

        private readonly Employee _staff;

        #endregion

        #region Observables

        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private AttendanceStatus _status = AttendanceStatus.Present;

        [ObservableProperty]
        private string? _leaveReason;

        [ObservableProperty]
        private string? _doctorsNotePath;

        [ObservableProperty]
        private TimeSpan? _clockInTime = new TimeSpan(7, 0, 0); // Default 07:00

        [ObservableProperty]
        private TimeSpan? _clockOutTime = new TimeSpan(16, 0, 0); // Default 16:00

        [ObservableProperty]
        private string _branch = string.Empty;

        [ObservableProperty]
        private bool _isOverrideEnabled;

        [ObservableProperty]
        private bool _isOnLeave;

        [ObservableProperty]
        private string _leaveType = string.Empty;

        #endregion

        #region Properties
        
        public Employee Staff => _staff; // Exposed for cloning/re-queueing
        public Guid EmployeeId => _staff.Id;
        public string Name => $"{_staff.FirstName} {_staff.LastName}";
        public string Role => _staff.Role.ToString();

        #endregion

        #region Constructors

        public StaffAttendanceViewModel(Employee staff)
        {
            _staff = staff;
            Branch = staff.Branch ?? "Johannesburg";
        }

        #endregion

        #region Computed Properties

        public double HoursWorked
        {
            get
            {
                if (ClockInTime.HasValue)
                {
                    // Assuming Today for active roll call / clock out
                    var inTime = DateTime.Today.Add(ClockInTime.Value);
                    var now = DateTime.Now;
                    if (now > inTime)
                        return (now - inTime).TotalHours;
                }
                return 0;
            }
        }
        
        public string HoursWorkedDisplay => HoursWorked > 0 ? $"{HoursWorked:F2}h" : "-";

        public decimal Wage
        {
            get
            {
                if (_staff.RateType == RateType.Hourly)
                {
                    return (decimal)(HoursWorked * _staff.HourlyRate);
                }
                return 0;
            }
        }
        
        public string WageDisplay => _staff.RateType == RateType.Hourly ? $"{Wage:C}" : "Salary";

        public void Refresh()
        {
            OnPropertyChanged(nameof(HoursWorked));
            OnPropertyChanged(nameof(HoursWorkedDisplay));
            OnPropertyChanged(nameof(Wage));
            OnPropertyChanged(nameof(WageDisplay));
        }

        #endregion
    }
}
