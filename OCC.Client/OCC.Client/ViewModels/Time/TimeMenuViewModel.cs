using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Time
{
    public partial class TimeMenuViewModel : ViewModelBase, IRecipient<SwitchTabMessage>
    {
        #region Observables

        [ObservableProperty]
        private string _activeTab = "Live";

        #endregion

        #region Constructors

        public TimeMenuViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
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
            // Optional: Only update if the message is relevant to Time views?
            // For now, simple like HomeMenuViewModel
            ActiveTab = message.Value;
        }

        #endregion
    }
}
