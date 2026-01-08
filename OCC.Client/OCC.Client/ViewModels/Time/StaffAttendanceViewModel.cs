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

        #endregion

        #region Properties

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
    }
}
