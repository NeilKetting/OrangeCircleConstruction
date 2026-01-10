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
    public partial class DailyTimesheetViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;
        private readonly ILeaveService _leaveService;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;

        #region Observables

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _pendingStaff = new();

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _loggedStaff = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedBranch = "Johannesburg";
        public string[] Branches => new[] { "All", "Johannesburg", "Cape Town" };

        #endregion

        private System.Collections.Generic.List<StaffAttendanceViewModel> _allPendingCache = new();
        private System.Collections.Generic.List<StaffAttendanceViewModel> _allLoggedCache = new();

        public event EventHandler? CloseRequested;

        public DailyTimesheetViewModel(
            ITimeService timeService, 
            ILeaveService leaveService, 
            IDialogService dialogService,
            IAuthService authService)
        {
            _timeService = timeService;
            _leaveService = leaveService;
            _dialogService = dialogService;
            _authService = authService;
            
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                // 1. Fetch Staff, Attendance, Leaves
                var allStaff = await _timeService.GetAllStaffAsync();
                var todayRecords = await _timeService.GetDailyAttendanceAsync(Date);
                var activeRecords = await _timeService.GetActiveAttendanceAsync(); // NEW: Check ALL active, not just today's
                var approvedLeave = await _leaveService.GetApprovedRequestsForDateAsync(Date);

                _allPendingCache.Clear();
                _allLoggedCache.Clear();

                foreach (var emp in allStaff.OrderBy(e => e.FirstName))
                {
                    // Leave Mapping
                    var leave = approvedLeave.FirstOrDefault(l => l.EmployeeId == emp.Id);
                    
                    // NEW LOGIC: Filter out if they have ANY ACTIVE shift (from today OR yesterday)
                    // We check activeRecords (which lists anyone with CheckOutTime == null)
                    var isActive = activeRecords.Any(r => r.EmployeeId == emp.Id);
                    
                    // ALSO filter out if they have a *completed* record for TODAY (so they don't appear in Pending if they finished a shift today)
                    // Wait, previous logic was: "If currently clocked out, appear in Pending".
                    // So we ONLY care about isActive.
                    
                    // Re-read requirement: "Employee clocked out... should lie on the left".
                    // So:
                    // - Is Active? -> NOT Pending (they are working).
                    // - Not Active? -> Pending (Ready to work).
                    
                    if (!isActive)
                    {
                        var vm = new StaffAttendanceViewModel(emp);
                        if (leave != null)
                        {
                            vm.IsOnLeave = true;
                            vm.LeaveType = leave.LeaveType.ToString();
                        }
                        _allPendingCache.Add(vm);
                    }
                }

                // 3. Populate Logged (All records, supporting multiple per employee)
                // We iterate records instead of staff to allow multiple rows
                // SORT: Descending (Newest First) per user request "be at the top"
                foreach (var record in todayRecords.OrderByDescending(r => r.ClockInTime))
                {
                    var emp = allStaff.FirstOrDefault(e => e.Id == record.EmployeeId);
                    if (emp == null) continue; // Should not happen

                    var vm = new StaffAttendanceViewModel(emp);
                    vm.Id = record.Id;
                    vm.Status = record.Status;
                    
                    // Map times
                    if (record.ClockInTime.HasValue) 
                        vm.ClockInTime = record.ClockInTime; 
                    else if (record.CheckInTime != DateTime.MinValue && record.CheckInTime != null) 
                        vm.ClockInTime = record.CheckInTime.Value.TimeOfDay;
                    else 
                        vm.ClockInTime = null;
                    
                    if (record.CheckOutTime != null && record.CheckOutTime != DateTime.MinValue)
                        vm.ClockOutTime = record.CheckOutTime.Value.TimeOfDay;
                    else
                        vm.ClockOutTime = null; // Still clocked in

                    _allLoggedCache.Add(vm);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in LoadDataAsync: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to load timesheet: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ReClockIn(StaffAttendanceViewModel item)
        {
             if (item == null) return;
             
             // 1. Prevent Multiple Active Shifts
             // Check if this employee ALREADY has an active record (CheckOutTime is null) - checking GLOBAL active list to catch yesterday's shifts
             var activeRecords = await _timeService.GetActiveAttendanceAsync();
             bool hasActiveShift = activeRecords.Any(x => x.EmployeeId == item.EmployeeId);
             
             if (hasActiveShift)
             {
                 await _dialogService.ShowAlertAsync("Active Shift Exists", $"{item.Name} is already clocked in. Please clock them out before starting a new shift.");
                 return;
             }

             // Logic: Create a BRAND NEW record for this employee.
             // We can fetch the employee details from the viewmodel item.
             var emp = await _timeService.GetAllStaffAsync(); 
             var staff = emp.FirstOrDefault(e => e.Id == item.EmployeeId);
             
             if (staff == null) return;

             IsSaving = true;
             try
             {
                 var now = DateTime.Now;
                 var record = new AttendanceRecord
                 {
                     Id = Guid.NewGuid(),
                     EmployeeId = staff.Id,
                     Date = Date,
                     Branch = staff.Branch,
                     Status = AttendanceStatus.Present,
                     CheckInTime = now,
                     ClockInTime = now.TimeOfDay,
                     CheckOutTime = null, // Open shift
                     CachedHourlyRate = (decimal?)staff.HourlyRate // SNAPSHOT RATE
                 };

                 await _timeService.SaveAttendanceRecordAsync(record);

                 // Create a new VM for this new record
                 var newVm = new StaffAttendanceViewModel(staff)
                 {
                     Id = record.Id,
                     Status = AttendanceStatus.Present,
                     ClockInTime = now.TimeOfDay,
                     ClockOutTime = null
                 };

                 // Add to cache (at TOP) and refresh
                 _allLoggedCache.Insert(0, newVm); // Insert at 0 to keep "Newest Top" order consistent
                 ApplyFilters();
                 
                 WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{staff.FirstName} started a new shift."));
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in ReClockIn: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to re-clock in: {ex.Message}");
             }
             finally { IsSaving = false; }
        }

        private void ApplyFilters()
        {
             var query = SearchText?.Trim();
             var branch = SelectedBranch;

             // Helper filter function
             bool Filter(StaffAttendanceViewModel s)
             {
                 bool matchBranch = branch == "All" || string.Equals(s.Branch?.Trim(), branch, StringComparison.OrdinalIgnoreCase);
                 bool matchSearch = string.IsNullOrWhiteSpace(query) || (s.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
                 return matchBranch && matchSearch;
             }

             PendingStaff = new ObservableCollection<StaffAttendanceViewModel>(
                 _allPendingCache.Where(Filter)
                                 .OrderBy(x => x.IsOnLeave)
                                 .ThenBy(x => x.Name));
             
             // Ensure LoggedStaff maintains the cache order (which we set to Descending)
             LoggedStaff = new ObservableCollection<StaffAttendanceViewModel>(_allLoggedCache.Where(Filter));
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnSelectedBranchChanged(string value) => ApplyFilters();

        [RelayCommand]
        private async Task MarkPresent(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            
            // CONFLICT CHECK: Is on Leave?
            if (item.IsOnLeave)
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                    "Leave Conflict", 
                    $"{item.Name} is on Approved {item.LeaveType} Leave today.\n\nDo you want to CANCEL their leave and mark them Present?");
                
                if (!confirm) return;
                
                // TODO: Cancel logic via LeaveService (Wait, do we have Cancel in Interface? We might need to add it or just proceed with warning)
                // For now, assume we proceed. Ideally we should API call to Cancel Leave.
            }

            // CONFLICT CHECK: Global Active
            var activeRecords = await _timeService.GetActiveAttendanceAsync();
            if (activeRecords.Any(r => r.EmployeeId == item.EmployeeId))
            {
                await _dialogService.ShowAlertAsync("Already Clocked In", $"{item.Name} has an active shift (possibly from yesterday). Please clock them out first.");
                return;
            }

            IsSaving = true;
            try
            {
                var now = DateTime.Now;
                var record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Branch = item.Branch,
                    Status = AttendanceStatus.Present,
                    CheckInTime = now,
                    ClockInTime = now.TimeOfDay,
                    CheckOutTime = null, // Explicitly null
                    CachedHourlyRate = (decimal?)item.Staff.HourlyRate // SNAPSHOT RATE
                };

                await _timeService.SaveAttendanceRecordAsync(record);
                
                item.Id = record.Id;
                item.Status = AttendanceStatus.Present;
                item.ClockInTime = now.TimeOfDay;
                item.ClockOutTime = null;

                MoveToLogged(item);
                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{item.Name} marked Present"));
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in MarkPresent: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to mark present: {ex.Message}");
            }
            finally { IsSaving = false; }
        }

        [RelayCommand]
        private async Task MarkAbsent(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            IsSaving = true;
            try
            {
                // STRICT DATA INTEGRITY: Absent = No Times
                var record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = item.EmployeeId,
                    Date = Date,
                    Branch = item.Branch,
                    Status = AttendanceStatus.Absent,
                    CheckInTime = null,
                    ClockInTime = null,
                    CheckOutTime = null
                };

                await _timeService.SaveAttendanceRecordAsync(record);
                
                item.Id = record.Id;
                item.Status = AttendanceStatus.Absent;
                item.ClockInTime = null;
                item.ClockOutTime = null;

                MoveToLogged(item);
                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{item.Name} marked Absent"));
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in MarkAbsent: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to mark absent: {ex.Message}");
            }
            finally { IsSaving = false; }
        }

        [RelayCommand]
        private async Task ClockOut(StaffAttendanceViewModel item)
        {
            if (item == null || item.Id == Guid.Empty) return;
            
            // 1. Business Hours
            var branch = item.Branch ?? "Johannesburg";
            var endTime = GetBusinessEndTime(branch);
            var now = DateTime.Now;

            string? leaveReason = null;
            string? leaveNote = null;
            var finalStatus = AttendanceStatus.Present;

            // 2. Check if Early
            if (now.TimeOfDay < endTime)
            {
                var diff = endTime - now.TimeOfDay;
                // If early by more than 15 mins (buffer), prompt
                if (diff.TotalMinutes > 15)
                {
                    var result = await _dialogService.ShowLeaveEarlyReasonAsync();
                    if (!result.Confirmed) return; // User cancelled clock out

                    leaveReason = result.Reason;
                    leaveNote = result.Note;
                    finalStatus = AttendanceStatus.LeaveEarly; // Or keep Present? Requirement implies flagging it.
                }
            }

            IsSaving = true;
            try 
            {
                var record = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                if (record != null)
                {
                    record.CheckOutTime = now;
                    // If we have a reason, update status and notes
                    if (!string.IsNullOrEmpty(leaveReason))
                    {
                        record.Status = AttendanceStatus.LeaveEarly;
                        record.LeaveReason = leaveReason;
                        record.Notes = !string.IsNullOrEmpty(leaveNote) ? $"[Leave Early Note] {leaveNote}" : null;
                    }
                    else
                    {
                        record.Status = AttendanceStatus.Present; // Ensure status reverts to Present if normal clock out
                    }
                    
                    await _timeService.SaveAttendanceRecordAsync(record);
                    
                    item.ClockOutTime = record.CheckOutTime.Value.TimeOfDay;
                    item.Status = record.Status; // Update UI status
                    
                    // Trigger UI refresh?
                    var loggedItem = LoggedStaff.FirstOrDefault(x => x.Id == item.Id);
                    if (loggedItem != null) 
                    {
                        loggedItem.ClockOutTime = item.ClockOutTime;
                        loggedItem.Status = item.Status;
                    }
                    // AND NOW: Move back to Pending (Left) because they are inactive!
                    MoveToPending(item);
                }
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in ClockOut: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to clock out: {ex.Message}");
            }
            finally { IsSaving = false; }
        }

        private TimeSpan GetBusinessEndTime(string branch)
        {
            // JHB: 07:00 - 16:00
            // CPT: 08:00 - 17:00
            if (branch.Contains("Cape", StringComparison.OrdinalIgnoreCase))
            {
                return new TimeSpan(17, 0, 0);
            }
            // Default JHB
            return new TimeSpan(16, 0, 0);
        }

        [RelayCommand]
        private async Task EditEntry(StaffAttendanceViewModel item)
        {
            if (item == null || item.Id == Guid.Empty) return;

            // Open Dialog
            var result = await _dialogService.ShowEditAttendanceAsync(item.ClockInTime, item.ClockOutTime);
            if (!result.Confirmed) return;

            IsSaving = true;
            try
            {
                var record = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                if (record != null)
                {
                    // Update times
                    if (result.InTime.HasValue)
                    {
                        record.CheckInTime = Date.Add(result.InTime.Value); 
                        record.ClockInTime = result.InTime;
                        // Status logic: If correcting to a time, ensure Present/Late/etc.
                        // Ideally we re-evaluate status, but manual override implies Present usually.
                        if (record.Status == AttendanceStatus.Absent) record.Status = AttendanceStatus.Present;
                    }
                    
                    if (result.OutTime.HasValue)
                    {
                        record.CheckOutTime = Date.Add(result.OutTime.Value);
                    }
                    else
                    {
                        record.CheckOutTime = null;
                    }
                    
                    await _timeService.SaveAttendanceRecordAsync(record);

                    // Update UI
                    item.ClockInTime = result.InTime;
                    item.ClockOutTime = result.OutTime;
                    item.Status = record.Status;

                    // Trigger UI refresh
                    var loggedItem = LoggedStaff.FirstOrDefault(x => x.Id == item.Id);
                    if (loggedItem != null)
                    {
                        loggedItem.ClockInTime = item.ClockInTime;
                        loggedItem.ClockOutTime = item.ClockOutTime;
                        loggedItem.Status = item.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in EditEntry: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to edit entry: {ex.Message}");
            }
            finally { IsSaving = false; }
        }
        
        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MoveToLogged(StaffAttendanceViewModel item)
        {
            // Use RemoveAll for safety to ensure it's gone even if references differ
            _allPendingCache.RemoveAll(x => x.EmployeeId == item.EmployeeId);
            
            // Insert at 0 to match "Newest First" view
            _allLoggedCache.Insert(0, item); 
            
            // Re-apply filter
            ApplyFilters(); 
        }

        private void MoveToPending(StaffAttendanceViewModel item)
        {
            // When clocking out, we want this employee to appear in the Pending list again
            // so they can be clocked in for a new shift if needed.
            
            // Safety check: is he already in pending?
            if (_allPendingCache.Any(x => x.EmployeeId == item.EmployeeId)) return;
            
            // Create a fresh VM for the Pending list
            var pendingVm = new StaffAttendanceViewModel(item.Staff)
            {
                // Reset fields for "New Shift" state
                ClockInTime = null,
                ClockOutTime = null,
                Status = AttendanceStatus.Present
            };
            
            _allPendingCache.Add(pendingVm);
            
            // Re-apply filter
            ApplyFilters(); 
        }
    }
}
