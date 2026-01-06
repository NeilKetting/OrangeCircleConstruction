using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Time
{
    public partial class RollCallViewModel : ViewModelBase
    {
        #region Private Members

        private readonly ITimeService _timeService;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? SaveCompleted;

        #endregion

        #region Observables

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _staffList = new();

        [ObservableProperty]
        private bool _isSaving;

        #endregion

        #region Constructors

        public RollCallViewModel(ITimeService timeService)
        {
            _timeService = timeService;
            _ = LoadStaff();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Save()
        {
            IsSaving = true;
            try
            {
                foreach (var item in StaffList)
                {
                    var record = new AttendanceRecord
                    {
                        Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                        EmployeeId = item.EmployeeId,
                        Date = Date,
                        Status = item.Status,
                        LeaveReason = item.LeaveReason,
                        DoctorsNoteImagePath = item.DoctorsNotePath
                    };
                    await _timeService.SaveAttendanceRecordAsync(record);
                }
                SaveCompleted?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private async Task LoadStaff()
        {
            var staff = await _timeService.GetAllStaffAsync();
            var existingRecords = (await _timeService.GetDailyAttendanceAsync(Date)).ToList();
            
            StaffList.Clear();
            foreach (var s in staff)
            {
                var vm = new StaffAttendanceViewModel(s);
                var existing = existingRecords.FirstOrDefault(r => r.EmployeeId == s.Id);
                if (existing != null)
                {
                    vm.Id = existing.Id;
                    vm.Status = existing.Status;
                    vm.LeaveReason = existing.LeaveReason;
                    vm.DoctorsNotePath = existing.DoctorsNoteImagePath;
                }
                StaffList.Add(vm);
            }
        }

        #endregion
    }
}
