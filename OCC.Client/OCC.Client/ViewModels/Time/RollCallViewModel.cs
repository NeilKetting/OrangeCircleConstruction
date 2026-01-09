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
        private readonly IDialogService _dialogService;

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

        [ObservableProperty]
        private string _searchText = string.Empty;

        private System.Collections.Generic.List<StaffAttendanceViewModel> _allStaff = new();

        // Branch Selection
        [ObservableProperty]
        private string _selectedBranch = "Johannesburg";
        
        public string[] Branches => new[] { "All", "Johannesburg", "Cape Town" };

        #endregion

        #region Constructors

        public RollCallViewModel(ITimeService timeService, IDialogService dialogService)
        {
            _timeService = timeService;
            _dialogService = dialogService;
            _ = LoadStaff();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task AttachNote(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            // PDF and Images (jpg, jpeg, png)
            var extensions = new[] { "*.pdf", "*.jpg", "*.jpeg", "*.png" };
            
            var file = await _dialogService.PickFileAsync("Attach Doctor's Note", extensions);
            if (!string.IsNullOrEmpty(file))
            {
                item.DoctorsNotePath = file;
                // Optionally extract filename for display? "Note Attached"
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
                    string? serverPath = item.DoctorsNotePath;

                    // If path is local (contains drive separator or not starting with /uploads), upload it
                    if (!string.IsNullOrEmpty(item.DoctorsNotePath) && 
                        (item.DoctorsNotePath.Contains(":") || !item.DoctorsNotePath.Replace("\\", "/").StartsWith("/uploads")))
                    {
                         serverPath = await _timeService.UploadDoctorNoteAsync(item.DoctorsNotePath);
                    }

                    var record = new AttendanceRecord
                    {
                        Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id,
                        EmployeeId = item.EmployeeId,
                        Date = Date,
                        Status = item.Status,
                        LeaveReason = item.LeaveReason ?? string.Empty,
                        DoctorsNoteImagePath = serverPath ?? string.Empty,
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
             if (item == null) return;
             IsSaving = true;
             try
             {
                 // Create a record that signifies "Not Here" or "Immediately Left".
                 // Initial thought: Mark as 'Absent'? 
                 // Or if the user clicked "Out" implies they are present but leaving?
                 // Context: Roll Call is usually "Who is here?". 
                 // If I click "Out", it likely means "Cancel" or "Mark as Absent" if they were not clocked in yet.
                 // BUT if they are not in the list (because active session filtered out), this button only appears for those NOT clocked in yet.
                 // So "Out" here likely means "Mark as Absent".
                 
                 var record = new AttendanceRecord
                 {
                     Id = Guid.NewGuid(),
                     EmployeeId = item.EmployeeId,
                     Date = Date,
                     Status = AttendanceStatus.Absent, // Mark as Absent
                     LeaveReason = "Marked Absent in Roll Call",
                     Branch = item.Branch
                 };

                 await _timeService.SaveAttendanceRecordAsync(record);
                 
                 // Notify Live View
                 WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Marked as Absent"));
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
            FilterStaff();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterStaff();
        }

        private void FilterStaff()
        {
            if (_allStaff == null) return;
            
            var query = SearchText?.Trim();
            var branch = SelectedBranch?.Trim();

            var filtered = _allStaff.AsEnumerable();

            // Filter by Branch
            if (!string.IsNullOrEmpty(branch) && !branch.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
               filtered = filtered.Where(s => string.Equals(s.Branch?.Trim(), branch, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Search Text
            if (!string.IsNullOrWhiteSpace(query))
            {
                filtered = filtered.Where(s => 
                    s.Name != null && s.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            StaffList = new ObservableCollection<StaffAttendanceViewModel>(filtered);
        }

        private async Task LoadStaff()
        {
            try
            {
                var staff = await _timeService.GetAllStaffAsync();
                // Sort by Name immediately
                var sortedStaff = staff.OrderBy(s => s.FirstName).ThenBy(s => s.LastName);
                
                var existingRecords = (await _timeService.GetDailyAttendanceAsync(Date)).ToList();
            
                _allStaff.Clear();

                foreach (var s in sortedStaff)
                {
                    // SPLIT SHIFT CHANGE:
                    // Check if they have an ACTIVE session (CheckIn but NO CheckOut)
                    var activeSession = existingRecords.FirstOrDefault(r => r.EmployeeId == s.Id && r.CheckOutTime == null);
                
                    // If active session exists, don't show in Roll Call (they must clock out active session first)
                    if (activeSession != null)
                    {
                        continue;
                    }
                
                    var vm = new StaffAttendanceViewModel(s);
                    _allStaff.Add(vm);
                }

                FilterStaff();
            }
            catch (Exception ex)
            {
                // Notify User of Error
                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"Error loading staff: {ex.Message}"));
                // Optionally log
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        #endregion
    }
}
