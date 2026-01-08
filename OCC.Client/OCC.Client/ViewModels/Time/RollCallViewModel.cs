using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;

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

        // Branch Selection
        [ObservableProperty]
        private string _selectedBranch = "Johannesburg";
        
        public string[] Branches => new[] { "All", "Johannesburg", "Cape Town" };

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
                        LeaveReason = item.LeaveReason ?? string.Empty,
                        DoctorsNoteImagePath = item.DoctorsNotePath ?? string.Empty,
                        // New Fields
                        Branch = item.Branch,
                        ClockInTime = item.ClockInTime
                    };
                    await _timeService.SaveAttendanceRecordAsync(record);
                }
                
                // Notify Live View?
                // For now, assume TimeViewModel reloads, or send message
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new ViewModels.Messages.UpdateStatusMessage("Attendance Saved"));

                SaveCompleted?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsSaving = false;
            }
        }



        [RelayCommand]
        private async Task ClockIndividual(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            IsSaving = true;
            try
            {
                DateTime checkInTime;
                if (item.IsOverrideEnabled && item.ClockInTime.HasValue)
                {
                    checkInTime = Date.Date.Add(item.ClockInTime.Value);
                }
                else
                {
                    checkInTime = DateTime.Now;
                }

                // Fetch existing to preserve other fields or creates new
                var dailyRecords = await _timeService.GetDailyAttendanceAsync(Date);
                var existing = dailyRecords.FirstOrDefault(r => r.EmployeeId == item.EmployeeId);
                
                var record = existing ?? new AttendanceRecord
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Branch = item.Branch
                };

                record.Status = AttendanceStatus.Present;
                record.CheckInTime = checkInTime;
                record.ClockInTime = checkInTime.TimeOfDay; // Sync legacy/VM field
                
                await _timeService.SaveAttendanceRecordAsync(record);
                
                // Notify Live View
                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Clocked In"));
                WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "Updated", record.Id));

                // Remove from list
                StaffList.Remove(item);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task ClockOutIndividual(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            IsSaving = true;
            try
            {
                DateTime checkOutTime;
                if (item.IsOverrideEnabled && item.ClockOutTime.HasValue)
                {
                     checkOutTime = Date.Date.Add(item.ClockOutTime.Value);
                }
                else
                {
                     checkOutTime = DateTime.Now;
                }
                
                var dailyRecords = await _timeService.GetDailyAttendanceAsync(Date);
                var existing = dailyRecords.FirstOrDefault(r => r.EmployeeId == item.EmployeeId);
                
                var record = existing ?? new AttendanceRecord
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Branch = item.Branch,
                    Status = AttendanceStatus.Present // Assume present if clocking out
                };
                
                record.CheckOutTime = checkOutTime;

                await _timeService.SaveAttendanceRecordAsync(record);

                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Clocked Out"));
                WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "Updated", record.Id));

                StaffList.Remove(item);
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

        partial void OnSelectedBranchChanged(string value)
        {
            _ = LoadStaff();
        }

        private async Task LoadStaff()
        {
            var staff = await _timeService.GetAllStaffAsync();
            var existingRecords = (await _timeService.GetDailyAttendanceAsync(Date)).ToList();
            
            // Filter by Branch
            if (!string.IsNullOrEmpty(SelectedBranch) && !SelectedBranch.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                staff = staff.Where(s => string.Equals(s.Branch?.Trim(), SelectedBranch.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            StaffList.Clear();
            foreach (var s in staff)
            {
                var existing = existingRecords.FirstOrDefault(r => r.EmployeeId == s.Id);
                
                // If they are already present/late (Checked In), skip them logic?
                // User said: "remove from the list" if clocked in.
                if (existing != null && (existing.Status == AttendanceStatus.Present || existing.Status == AttendanceStatus.Late))
                {
                    continue;
                }

                var vm = new StaffAttendanceViewModel(s);
                if (existing != null)
                {
                    // Maybe they are Absent or Sick, so we keep them in list to potentially update?
                    // But if they have a record, user might want to edit.
                    // For now, let's just filter PRESENT ones as "Done".
                    vm.Id = existing.Id;
                    vm.Status = existing.Status;
                    vm.LeaveReason = existing.LeaveReason;
                    vm.DoctorsNotePath = existing.DoctorsNoteImagePath;
                    vm.ClockInTime = existing.ClockInTime ?? new TimeSpan(7,0,0);
                }
                StaffList.Add(vm);
            }
        }

        #endregion
    }
}
