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
        
        public string[] Branches => new[] { "Johannesburg", "Cape Town" }; // Hardcoded for now based on request

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
                var record = new AttendanceRecord
                {
                    Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Status = AttendanceStatus.Present, // Force Present on "Clock" button
                    LeaveReason = string.Empty,
                    DoctorsNoteImagePath = string.Empty,
                    Branch = item.Branch,
                    ClockInTime = DateTime.Now.TimeOfDay, // Use current time
                    CheckInTime = DateTime.Now // Also set DateTime for robustness
                };
                
                await _timeService.SaveAttendanceRecordAsync(record);
                
                // Notify Live View
                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Attendance Saved"));
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
            if (!string.IsNullOrEmpty(SelectedBranch))
            {
                staff = staff.Where(s => s.Branch == SelectedBranch);
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
