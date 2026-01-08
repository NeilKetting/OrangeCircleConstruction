using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Time
{
    public partial class TimeAttendanceViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IAuthService _authService;

        #endregion

        #region Observables

        [ObservableProperty]
        private TimeMenuViewModel _timeMenu;

        [ObservableProperty]
        private ViewModelBase _currentView;

        // Sub-ViewModels
        [ObservableProperty] private TimeLiveViewModel _liveView;
        [ObservableProperty] private RollCallViewModel _dailyRollCallView;
        [ObservableProperty] private LeaveApplicationViewModel _leaveApplicationView;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

        #endregion

        #region Constructors

        public TimeAttendanceViewModel()
        {
            // Design-time
            _timeMenu = null!;
            _liveView = null!;
            _dailyRollCallView = null!;
            _leaveApplicationView = null!;
            _currentView = null!;

            _authService = null!;
        }

        public TimeAttendanceViewModel(
            TimeMenuViewModel timeMenu,
            TimeLiveViewModel liveView,
            RollCallViewModel dailyRollCallView,
            LeaveApplicationViewModel leaveApplicationView,
            IAuthService authService)
        {
            _timeMenu = timeMenu;
            _liveView = liveView;
            _dailyRollCallView = dailyRollCallView;
            _leaveApplicationView = leaveApplicationView;
            _authService = authService;
            
            // Default View
            _currentView = _liveView;

            // Ensure Tab selection matches default view
            _timeMenu.ActiveTab = "Live";

            _timeMenu.PropertyChanged += TimeMenu_PropertyChanged;
            
            // Handle Close/Save from Daily Roll Call tab
            _dailyRollCallView.CloseRequested += (s, e) => { TimeMenu.ActiveTab = "Live"; };
            _dailyRollCallView.SaveCompleted += (s, e) => { TimeMenu.ActiveTab = "Live"; };

            Initialize();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            var now = DateTime.Now;
            Greeting = GetGreeting(now);
            CurrentDate = now.ToString("dd MMMM yyyy");
            UpdateVisibility();
        }

        private void TimeMenu_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeMenuViewModel.ActiveTab))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            switch (TimeMenu.ActiveTab)
            {
                case "Daily Roll Call":
                    CurrentView = DailyRollCallView;
                    break;
                case "Leave Application":
                    CurrentView = LeaveApplicationView;
                    break;
                case "Live":
                default:
                    CurrentView = LiveView;
                    break;
            }
        }

        private string GetGreeting(DateTime time)
        {
             string timeGreeting = time.Hour < 12 ? "Good morning" :
                                  time.Hour < 18 ? "Good afternoon" : "Good evening";

            var userName = _authService.CurrentUser?.DisplayName ?? "User";
            return $"{timeGreeting}, {userName}";
        }

        #endregion
    }
}
