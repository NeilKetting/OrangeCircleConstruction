using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class ClockOutViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;

        #region Observables

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _staffList = new();

        [ObservableProperty]
        private ObservableCollection<string> _branches = new();

        [ObservableProperty]
        private string _selectedBranch = "All"; // Default to All

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSaving;

        #endregion

        private System.Collections.Generic.List<StaffAttendanceViewModel> _allLoadedStaff = new();

        public ClockOutViewModel(ITimeService timeService)
        {
            _timeService = timeService;
            InitializeCommand.Execute(null);
        }

        [RelayCommand]
        private async Task Initialize()
        {
            IsLoading = true;
            try
            {
                await LoadDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            var date = DateTime.Today;
            var allStaff = await _timeService.GetAllStaffAsync();
            var dailyRecords = await _timeService.GetDailyAttendanceAsync(date);

            // 1. Populate Branches (Only once or if needed, but safe here if we don't trigger change)
            var branches = allStaff.Select(s => s.Branch ?? "Unassigned").Distinct().OrderBy(b => b).ToList();
            branches.Insert(0, "All");
            
            // Only update Branches if different to avoid binding triggers
            if (Branches.Count != branches.Count || !Branches.SequenceEqual(branches))
            {
                Branches = new ObservableCollection<string>(branches);
            }
             
            if (!Branches.Contains(SelectedBranch)) SelectedBranch = "All";

            // 2. Build Cache of Display Models
            _allLoadedStaff.Clear();

            // Filter: Only those who are PRESENT or LATE and NOT yet clocked out
            var clockedInRecords = dailyRecords
                .Where(r => (r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late) 
                         && r.CheckOutTime == null)
                .ToList();

            foreach (var record in clockedInRecords)
            {
                var staff = allStaff.FirstOrDefault(e => e.Id == record.EmployeeId);
                if (staff == null) continue;

                var vm = new StaffAttendanceViewModel(staff)
                {
                    Id = record.Id,
                    Status = record.Status,
                    ClockInTime = record.CheckInTime?.TimeOfDay ?? record.ClockInTime,
                    Branch = staff.Branch ?? "Unassigned"
                };
                _allLoadedStaff.Add(vm);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allLoadedStaff == null) return;

            var filtered = _allLoadedStaff.Where(s => SelectedBranch == "All" || s.Branch == SelectedBranch);
            StaffList = new ObservableCollection<StaffAttendanceViewModel>(filtered);
        }

        // Re-load when filter changes
        partial void OnSelectedBranchChanged(string value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private async Task ClockOut(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.LeaveReason))
            {
                // In a real app we might show a dialog or validation error.
                // For now, we rely on the UI 'watermark' or maybe a quick check.
                // Assuming the View binds to LeaveReason on the VM.
                
                // Let's enforce it:
                 // TODO: Show notification "Reason Required"
                 return;
            }

            IsSaving = true;
            try
            {
                var record = new AttendanceRecord
                {
                    Id = item.Id,
                    EmployeeId = item.EmployeeId,
                    Date = DateTime.Today,
                    Status = AttendanceStatus.LeaveEarly,
                    CheckOutTime = DateTime.Now,
                    LeaveReason = item.LeaveReason,
                    Branch = item.Branch
                    // Start time is preserved in DB merge or we should fetch existing?
                    // The API Update method usually replaces. 
                    // Best practice: Fetch, Update, Save.
                };

                // Fetch existing to be safe
                var dailyRecords = await _timeService.GetDailyAttendanceAsync(DateTime.Today);
                var existing = dailyRecords.FirstOrDefault(r => r.Id == item.Id);
                if (existing != null)
                {
                    existing.CheckOutTime = DateTime.Now;
                    existing.Status = AttendanceStatus.LeaveEarly;
                    existing.LeaveReason = item.LeaveReason;
                    await _timeService.SaveAttendanceRecordAsync(existing);
                }
                else
                {
                     // Should exist if they are in this list, but fallback logic:
                    await _timeService.SaveAttendanceRecordAsync(record);
                }

                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Clocked Out Early"));
                WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "Updated", item.Id));

                StaffList.Remove(item);
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
