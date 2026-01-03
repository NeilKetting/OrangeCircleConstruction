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
        private readonly ITimeService _timeService;
        
        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _staffList = new();

        [ObservableProperty]
        private bool _isSaving;

        public event EventHandler? CloseRequested;
        public event EventHandler? SaveCompleted;

        public RollCallViewModel(ITimeService timeService)
        {
            _timeService = timeService;
            _ = LoadStaff();
        }

        private async Task LoadStaff()
        {
            var staff = await _timeService.GetAllStaffAsync();
            var existingRecords = (await _timeService.GetDailyAttendanceAsync(Date)).ToList();
            
            StaffList.Clear();
            foreach (var s in staff)
            {
                var vm = new StaffAttendanceViewModel(s);
                var existing = existingRecords.FirstOrDefault(r => r.StaffId == s.Id);
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
                        StaffId = item.StaffId,
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
    }
}
