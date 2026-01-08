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

                // Split Shift Logic: Always create NEW record if we are clocking in via Roll Call
                // UNLESS we are editing a specific ID (which shouldn't happen here if we refined LoadStaff).
                // Actually, if we are in Roll Call, we assume we are starting a SESSION.
                // If there is an open session, we filter it out in LoadStaff.
                // So here, we just Create New.
                
                var record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(), // Always new session
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Branch = item.Branch,
                    Status = AttendanceStatus.Present,
                    CheckInTime = checkInTime,
                    ClockInTime = checkInTime.TimeOfDay
                };

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
        
        // ... ClockOutIndividual remains same or removed if generic? 
        // Actually ClockOutIndividual is in this file too? 
        // Wait, RollCallViewModel has ClockOutIndividual? 
        // Yes, likely for "correction" or if we showed active people. 
        // But LoadStaff filters out active people. 
        // Let's keep ClockOutIndividual just in case logic changes, but ensure LoadStaff is key.

        [RelayCommand]
        private async Task ClockOutIndividual(StaffAttendanceViewModel item)
        {
             // If we are allowing Clock Out from here, we need Id.
             // But simpler to rely on ClockOutViewModel for that.
             if (item == null) return;
             await Task.CompletedTask;
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
                // SPLIT SHIFT CHANGE:
                // Check if they have an ACTIVE session (CheckIn but NO CheckOut)
                var activeSession = existingRecords.FirstOrDefault(r => r.EmployeeId == s.Id && r.CheckOutTime == null);
                
                // If they are currently clocked in, DO NOT SHOW in Roll Call (they must clock out first)
                if (activeSession != null)
                {
                    continue;
                }

                // If they have previous CLOSED sessions, that's fine. We show them so they can start a NEW session.
                
                var vm = new StaffAttendanceViewModel(s);
                // We DO NOT map ID here, because we want a NEW record if they clock in.
                // UNLESS... do we want to support "Resume"? No, that's complex. New Session is cleaner.
                
                // Pre-populate Shift Start Time from Employee settings (not previous record)
                // If they have a shift set on employee profile, use it? 
                // Currently just defaults to 7:00 in VM ctor or similar.
                
                StaffList.Add(vm);
            }
        }

        #endregion
    }
}
