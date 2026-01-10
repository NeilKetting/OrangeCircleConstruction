using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Notifications;
using System;

namespace OCC.Client.ViewModels.Orders
{
    public partial class OrderMenuViewModel : ViewModelBase, IRecipient<SwitchTabMessage>
    {
        #region Observables

        [ObservableProperty]
        private string _activeTab = "Dashboard";

        [ObservableProperty]
        private string _userEmail = "origize63@gmail.com";

        #endregion

        #region Constructors

        public NotificationViewModel NotificationVM { get; }

        public event EventHandler<string>? TabSelected;

        public OrderMenuViewModel(NotificationViewModel notificationVM, IAuthService authService)
        {
            NotificationVM = notificationVM;
            // Removed WeakReferenceMessenger if not used for parent comms, but keeping for now if other things rely on it.
            WeakReferenceMessenger.Default.RegisterAll(this);
            
            if (authService.CurrentUser != null)
            {
                UserEmail = authService.CurrentUser.Email;
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
            TabSelected?.Invoke(this, tabName);
        }

        [RelayCommand]
        private void OpenNotifications()
        {
            WeakReferenceMessenger.Default.Send(new OpenNotificationsMessage());
        }

        #endregion

        #region Methods

        public void Receive(SwitchTabMessage message)
        {
            ActiveTab = message.Value;
        }

        #endregion
    }
}
