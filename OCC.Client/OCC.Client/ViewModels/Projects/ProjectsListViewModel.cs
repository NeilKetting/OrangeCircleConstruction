using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.ViewModels.Projects.Dashboard;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectsListViewModel : ViewModelBase
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<ProjectDashboardItemViewModel> _projects = new();

        public ProjectsListViewModel(IRepository<Project> projectRepository, IDialogService dialogService)
        {
            _projectRepository = projectRepository;
            _dialogService = dialogService;
        }

        public ProjectsListViewModel()
        {
            // Design-time
            _projectRepository = null!;
            _dialogService = null!;
            Projects = new ObservableCollection<ProjectDashboardItemViewModel>
            {
                new ProjectDashboardItemViewModel { Name = "Construction Schedule", Progress = 7, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2026, 7, 13) },
                new ProjectDashboardItemViewModel { Name = "Engen", Progress = 97, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2025, 11, 6) }
            };
        }

        [RelayCommand]
        public async Task LoadProjects()
        {
            if (_projectRepository == null) return;

            try 
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Loading Projects...");
                var projects = await _projectRepository.GetAllAsync();
                
                // Transform to dashboard items
                // This is a mockup transformation since we might not have all this data in the plain Project model yet
                var dashboardItems = projects.Select(p => new ProjectDashboardItemViewModel
                {
                    Name = p.Name,
                    Progress = 0, // Mockup
                    ProjectManagerInitials = "OR", // Mockup
                    Members = new() { "OR" }, // Mockup
                    Status = "Planning", // Mockup
                    LatestFinish = p.EndDate
                });

                Projects = new ObservableCollection<ProjectDashboardItemViewModel>(dashboardItems);
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Load Projects Complete. Count: {Projects.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] CRASH in LoadProjects: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Stack: {ex.StackTrace}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading projects: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void NewProject()
        {
            // Logic to create new project (maybe navigate to wizard)
        }
    }
}
