using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.Services;
using System;
using Avalonia.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace OCC.Client.ViewModels
{
    public partial class NotificationViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Notification> _notifications = new();

        [ObservableProperty]
        private bool _hasNotifications;

        [ObservableProperty]
        private bool _isAdmin;

        private readonly SignalRNotificationService _signalRService;
        private readonly IAuthService _authService;

        public NotificationViewModel(SignalRNotificationService signalRService, IAuthService authService)
        {
            _signalRService = signalRService;
            _authService = authService;
            
            // Check Admin Role
            IsAdmin = _authService.CurrentUser?.UserRole == OCC.Shared.Models.UserRole.Admin;

            // Initialize with empty list for now
            HasNotifications = Notifications.Count > 0;
            Notifications.CollectionChanged += (s, e) => HasNotifications = Notifications.Count > 0;

            _signalRService.OnNotificationReceived += OnNotificationReceived;
            _ = _signalRService.StartAsync(); // Start Connection
        }

        public NotificationViewModel()
        {
             // Design time
        }

        private void OnNotificationReceived(string message)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Notifications.Insert(0, new Notification 
                { 
                    Title = "New Registration", 
                    Message = message, 
                    Timestamp = DateTime.Now,
                    IsRead = false 
                });
            });
        }

        [RelayCommand]
        private async Task ApproveUserAsync()
        {
            // TODO: Implement actual API call to approve user. 
            // For now, we will simply remove the notification to simulate action.
            // In a real scenario, the notification might contain the UserId to approve.
            
            // Example Placeholder Logic:
            if (Notifications.Count > 0)
            {
                Notifications.RemoveAt(0);
            }
        }
    }
}
