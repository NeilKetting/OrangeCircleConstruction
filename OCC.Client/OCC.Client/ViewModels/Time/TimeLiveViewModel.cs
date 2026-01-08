using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Messages;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using System.Collections.Generic;
using Avalonia.Threading;

namespace OCC.Client.ViewModels.Time
{
    public partial class TimeLiveViewModel : ViewModelBase, IRecipient<UpdateStatusMessage>, IRecipient<EntityUpdatedMessage>
    {

        #region Private Members

        private readonly ITimeService _timeService;
        private readonly IAuthService _authService;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<LiveUserCardViewModel> _liveUsers = new();

        [ObservableProperty]
        private string _lastUpdatedText = "Updated on " + DateTime.Now.ToString("dd MMMM yyyy HH:mmtt").ToLower();

        [ObservableProperty]
        private bool _isRollCallVisible = false;

        [ObservableProperty]
        private RollCallViewModel? _currentRollCall;

        #endregion

        #region Constructors

        public TimeLiveViewModel()
        {
            // Parameterless constructor for design-time support
            _timeService = null!;
            _authService = null!;
        }

        public TimeLiveViewModel(ITimeService timeService, IAuthService authService)
        {
            _timeService = timeService;
            _authService = authService;
            
            InitializeCommand.Execute(null);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Initialize()
        {
            LastUpdatedText = "Updated on " + DateTime.Now.ToString("dd MMMM yyyy HH:mmtt").ToLower();
            await LoadLiveData();
        }

        [RelayCommand]
        private void OpenRollCall()
        {
            CurrentRollCall = new RollCallViewModel(_timeService);
            CurrentRollCall.CloseRequested += (s, e) => CloseRollCall();
            IsRollCallVisible = true;
        }

        [RelayCommand]
        private void CloseRollCall()
        {
            IsRollCallVisible = false;
            CurrentRollCall = null;
        }

        [RelayCommand]
        private async Task ClearAttendance()
        {
            await _timeService.ClearAllAttendanceAsync();
            // Trigger refresh via message or direct reload
            await Initialize();
            
            // Send message to notify others?
             WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "ClearAll", Guid.Empty));
        }

        #endregion

        #region Methods

        private async Task LoadLiveData()
        {
            try
            {
                var allStaff = await _timeService.GetAllStaffAsync();
                var todayAttendance = await _timeService.GetDailyAttendanceAsync(DateTime.Today);

                var userViewModels = new List<LiveUserCardViewModel>();

                // Only show employees that have an attendance record for today (meaning Roll Call was done)
                foreach (var attendance in todayAttendance)
                {
                    var employee = allStaff.FirstOrDefault(e => e.Id == attendance.EmployeeId);
                    if (employee == null) continue;

                    // Determine status
                    bool isPresent = attendance.Status == AttendanceStatus.Present || attendance.Status == AttendanceStatus.Late;
                    TimeSpan? clockIn = attendance.CheckInTime?.TimeOfDay ?? attendance.ClockInTime;

                    var vm = new LiveUserCardViewModel
                    {
                        EmployeeId = employee.Id,
                        DisplayName = $"{employee.FirstName} {employee.LastName}"
                    };
                    vm.SetStatus(isPresent, clockIn);

                    userViewModels.Add(vm);
                }

                // Update Collection on UI Thread to be safe, though usually this runs on UI context
                Dispatcher.UIThread.Post(() =>
                {
                    LiveUsers.Clear();
                    // Sort by Name for nicer display
                    foreach (var u in userViewModels.OrderBy(x => x.DisplayName))
                    {
                        LiveUsers.Add(u);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading live data: {ex.Message}");
            }
        }

        #endregion

        public void Receive(UpdateStatusMessage message)
        {
            if (message.Value == "Attendance Saved")
            {
                _ = Initialize();
            }
        }

        public void Receive(EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "AttendanceRecord")
            {
                // Real-time update from any client
                Dispatcher.UIThread.InvokeAsync(Initialize);
            }
        }

    }
}
