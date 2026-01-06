using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Collections.ObjectModel;

namespace OCC.Client.ViewModels.Home.Dashboard
{
    public partial class PulseViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Project> _projectRepository;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<ProjectPulseItem> _projects = new();

        #endregion

        #region Constructors

        public PulseViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public PulseViewModel(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
            LoadProjects();
        }

        #endregion

        #region Methods

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

        #endregion
    }

    public class ProjectPulseItem
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
    }
}
