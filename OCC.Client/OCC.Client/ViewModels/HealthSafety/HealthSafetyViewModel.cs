using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class HealthSafetyViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private HealthSafetyDashboardViewModel _dashboardView;

        public HealthSafetyViewModel()
        {
            // Design-time
            _dashboardView = null!;
            _currentView = null!;
        }

        public HealthSafetyViewModel(HealthSafetyDashboardViewModel dashboardView)
        {
            _dashboardView = dashboardView;
            _currentView = _dashboardView;
        }
    }
}
