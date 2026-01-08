using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.HealthSafety;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.Settings;
using OCC.Client.ViewModels; // For LoginViewModel
using OCC.Client.ViewModels.Shared; // For ProfileViewModel
using OCC.Client.ViewModels.Messages; // Corrected Namespace
using OCC.Client.Services;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Infrastructure;
using OCC.Client.ViewModels.Notifications;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Core
{
    public partial class ShellViewModel : ViewModelBase, IRecipient<OpenNotificationsMessage>
    {
        #region Private Members

        private readonly IServiceProvider _serviceProvider;
        private readonly IPermissionService _permissionService;
        private readonly SignalRNotificationService _signalRService;

        #endregion

        #region Observables

        [ObservableProperty]
        private SideMenuViewModel _sideMenuViewModel;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        [ObservableProperty]
        private NotificationViewModel _notificationVM;

        [ObservableProperty]
        private bool _isNotificationOpen;

        [ObservableProperty]
        private bool _isAuthenticated;

        [ObservableProperty]
        private int _onlineCount;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<UserDisplayModel> _connectedUsers = new();

        #endregion

        #region Constructors

        public ShellViewModel()
        {
            // Parameterless constructor for design-time support
            _serviceProvider = null!;
            _permissionService = null!;
            _sideMenuViewModel = null!;
            _currentPage = null!;
            _signalRService = null!;
            _notificationVM = null!;
        }

        public ShellViewModel(
            IServiceProvider serviceProvider, 
            SideMenuViewModel sideMenuViewModel, 
            IUpdateService updateService, 
            IPermissionService permissionService,
            SignalRNotificationService signalRService)
        {
            _serviceProvider = serviceProvider;
            _permissionService = permissionService;
            _signalRService = signalRService;
            _signalRService.OnUserListReceived += OnUserUiUpdate;

            _sideMenuViewModel = sideMenuViewModel;
            _sideMenuViewModel.PropertyChanged += SideMenu_PropertyChanged;

            _currentPage = null!; // Silence warning as NavigateTo sets it
            
            // Initialize persistent Notification ViewModel
            _notificationVM = _serviceProvider.GetRequiredService<NotificationViewModel>();

            // Default to Home (Dashboard) unless Beta Notice is pending
            var currentVersion = updateService.CurrentVersion;
            
            if (!DevelopmentToBeDeleted.BetaNoticeViewModel.IsNoticeAccepted(currentVersion))
            {
                var betaVM = new DevelopmentToBeDeleted.BetaNoticeViewModel(currentVersion);
                betaVM.Accepted += () => 
                {
                    NavigateTo(Infrastructure.NavigationRoutes.Home);
                };
                CurrentPage = betaVM;
            }
            else
            {
                NavigateTo(Infrastructure.NavigationRoutes.Home);
            }

            WeakReferenceMessenger.Default.RegisterAll(this); // Register for messages

            // Start SignalR Connection Globally
            _ = _signalRService.StartAsync();

            // Check for updates in background, but show UI if found
            Task.Run(async () => 
            {
                var updateInfo = await updateService.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                    {
                        var dialog = new Views.Shared.UpdateDialogView();
                        dialog.DataContext = new ViewModels.Shared.UpdateDialogViewModel(updateService, updateInfo, () => dialog.Close());
                        
                        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null) 
                        {
                            dialog.ShowDialog(desktop.MainWindow);
                        }
                    });
                }
            });
        }

        #endregion

        #region Helper Methods

        private void SideMenu_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SideMenuViewModel.ActiveSection))
            {
                NavigateTo(SideMenuViewModel.ActiveSection);
            }
        }

        private void NavigateTo(string section)
        {
            // Close notification popup when navigating
            IsNotificationOpen = false;

            if (!_permissionService.CanAccess(section))
            {
                if (section != Infrastructure.NavigationRoutes.Home)
                {
                    NavigateTo(Infrastructure.NavigationRoutes.Home);
                }
                return;
            }

            switch (section)
            {
                case Infrastructure.NavigationRoutes.Home:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.StaffManagement:
                    CurrentPage = _serviceProvider.GetRequiredService<EmployeeManagementViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Projects:
                    CurrentPage = _serviceProvider.GetRequiredService<ProjectsViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Time:
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Time.TimeAttendanceViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Calendar: 
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Home.Calendar.CalendarViewModel>();
                    break;
                case "UserManagement":
                    CurrentPage = _serviceProvider.GetRequiredService<UserManagementViewModel>();
                    break;
                case "MyProfile":
                    CurrentPage = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    break;
                case "HealthSafety":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.HealthSafety.HealthSafetyViewModel>();
                    break;
                 default:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
            }
        }

        public void Receive(OpenNotificationsMessage message)
        {
            // Toggle visibility
            IsNotificationOpen = !IsNotificationOpen;
        }

        [RelayCommand]
        public void CloseNotifications()
        {
            IsNotificationOpen = false;
        }

        private void OnUserUiUpdate(System.Collections.Generic.List<OCC.Shared.DTOs.UserConnectionInfo> users)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                ConnectedUsers.Clear();
                foreach (var u in users) 
                {
                    var timeOnline = DateTime.UtcNow - u.ConnectedAt;
                    var timeStr = timeOnline.TotalMinutes < 1 ? "Just now" : 
                                  timeOnline.TotalHours < 1 ? $"{timeOnline.Minutes}m" : 
                                  $"{timeOnline.Hours}h {timeOnline.Minutes}m";

                    ConnectedUsers.Add(new UserDisplayModel(u.UserName, timeStr));
                }
                OnlineCount = users.Count;
            });
        }
        
        public record UserDisplayModel(string Name, string TimeOnline);

        #endregion
    }
}
