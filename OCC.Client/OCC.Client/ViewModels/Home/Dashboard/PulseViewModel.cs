using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Collections.ObjectModel;

namespace OCC.Client.ViewModels.Home.Dashboard
{
    public partial class PulseViewModel : ViewModelBase
    {
        private readonly IRepository<Project> _projectRepository;

        [ObservableProperty]
        private ObservableCollection<ProjectPulseItem> _projects = new();

        public PulseViewModel(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
            LoadProjects();
        }

        private async void LoadProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            Projects.Clear();
            foreach (var p in projects)
            {
                Projects.Add(new ProjectPulseItem
                {
                    ProjectName = p.Name,
                    Status = p.Status,
                    Progress = "50%", // Dummy for now
                    DueDate = p.EndDate.ToString("MMM dd")
                });
            }
        }
    }

    public class ProjectPulseItem
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
    }
}
