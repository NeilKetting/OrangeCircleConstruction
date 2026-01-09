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
        [NotifyPropertyChangedFor(nameof(IsClockSystemActive))]
        [NotifyPropertyChangedFor(nameof(IsLeaveActive))]
        [NotifyPropertyChangedFor(nameof(IsOvertimeActive))]
        private string _activeTab = "Live";

        public bool IsClockSystemActive => ActiveTab is "Daily Roll Call" or "Clock Out" or "History";
        public bool IsLeaveActive => ActiveTab is "Leave Application" or "LeaveApprovals";
        public bool IsOvertimeActive => ActiveTab is "Overtime" or "OvertimeApproval";

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
        }

        private readonly IPermissionService _permissionService;

        public bool CanApproveLeave => _permissionService.CanAccess("LeaveApprovals");
        public bool CanRequestOvertime => _permissionService.CanAccess("OvertimeRequest");
        public bool CanApproveOvertime => _permissionService.CanAccess("OvertimeApproval");

        public TimeMenuViewModel(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public TimeMenuViewModel()
        {
             _permissionService = null!;
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
