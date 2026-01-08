using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.ViewModels.Time
{
    public partial class TimeMenuViewModel : ViewModelBase, IRecipient<SwitchTabMessage>
    {
        #region Observables

        [ObservableProperty]
        private string _activeTab = "Live";

        #endregion

        #region Constructors

        private readonly IPermissionService _permissionService;

        public bool CanApproveLeave => _permissionService.CanAccess("LeaveApprovals");
        public bool CanRequestOvertime => _permissionService.CanAccess("OvertimeRequest");
        public bool CanApproveOvertime => _permissionService.CanAccess("OvertimeApproval");

        public TimeMenuViewModel(IPermissionService permissionService)
        {
            _permissionService = permissionService;
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public TimeMenuViewModel()
        {
             _permissionService = null!;
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
