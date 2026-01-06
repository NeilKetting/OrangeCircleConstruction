using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class NewTaskPopupViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<User> _userRepository; 
        private readonly IAuthService _authService;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler<Guid>? TaskCreated;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _taskName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private Project? _selectedProject;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private User? _assignedUser;

        #endregion

        #region Constructors

        public NewTaskPopupViewModel(IRepository<ProjectTask> taskRepository, 
                                     IRepository<Project> projectRepository,
                                     IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task CreateTask()
        {
            if (string.IsNullOrWhiteSpace(TaskName)) return;

            var newTask = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Name = TaskName,
                Description = "",
                StartDate = DateTime.Now,
                FinishDate = DateTime.Now.AddDays(1),
                ProjectId = SelectedProject?.Id ?? Guid.Empty, // Should force validation really
                AssignedTo = AssignedUser?.DisplayName ?? "UN",
                Status = "To Do",
                Priority = "Medium"
            };

            await _taskRepository.AddAsync(newTask);
            TaskCreated?.Invoke(this, newTask.Id);
            Close();
        }

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public async Task LoadData()
        {
            var projects = await _projectRepository.GetAllAsync();
            Projects = new ObservableCollection<Project>(projects);
            
            if (_authService.CurrentUser != null)
            {
               Users.Add(_authService.CurrentUser);
               AssignedUser = _authService.CurrentUser;
            }
        }

        #endregion
    }
}
