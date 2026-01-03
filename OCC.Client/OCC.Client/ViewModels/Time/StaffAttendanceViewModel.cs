using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;

namespace OCC.Client.ViewModels.Time
{
    public partial class StaffAttendanceViewModel : ViewModelBase
    {
        private readonly StaffMember _staff;

        [ObservableProperty]
        private Guid _id;

        public Guid StaffId => _staff.Id;
        public string Name => _staff.DisplayName;
        public string Role => _staff.Role.ToString();

        [ObservableProperty]
        private AttendanceStatus _status = AttendanceStatus.Present;

        [ObservableProperty]
        private string? _leaveReason;

        [ObservableProperty]
        private string? _doctorsNotePath;

        public StaffAttendanceViewModel(StaffMember staff)
        {
            _staff = staff;
        }
    }
}
