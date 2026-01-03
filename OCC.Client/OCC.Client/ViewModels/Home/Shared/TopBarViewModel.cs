using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Messages;

namespace OCC.Client.ViewModels.Home.Shared
{
    public partial class TopBarViewModel : ViewModelBase, IRecipient<SwitchTabMessage>
    {
        [ObservableProperty]
        private string _activeTab = "My Summary";


        [ObservableProperty]
        private string _userEmail = "origize63@gmail.com";

        public TopBarViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
        }

        public void Receive(SwitchTabMessage message)
        {
            ActiveTab = message.Value;
        }
    }
}
