using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
// using OCC.Client.ViewModels.Projects.Dashboard; // Removed
using OCC.Client.ViewModels.Projects.Shared;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ProjectMainMenuViewModel _projectMainMenu;

        [ObservableProperty]
        private ViewModelBase _currentView;

        public ProjectsViewModel(
            ProjectMainMenuViewModel projectMenu, 
            ProjectsListViewModel projectsListVM)
        {
            _projectMainMenu = projectMenu;
            _currentView = projectsListVM;
        }
    }
}
