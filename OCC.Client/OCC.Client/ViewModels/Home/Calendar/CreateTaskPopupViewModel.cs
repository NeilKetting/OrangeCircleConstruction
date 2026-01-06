using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Home.Calendar
{
    public partial class CreateTaskPopupViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? TaskCreated;

        #endregion

        #region Observables

        [ObservableProperty]
        private TaskType _selectedType = TaskType.Task;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private Project? _selectedProject;

        [ObservableProperty]
        private DateTime _dueDate;

        #endregion

        #region Properties

        public ObservableCollection<TaskType> TaskTypes { get; } = new ObservableCollection<TaskType>(Enum.GetValues<TaskType>());
        public ObservableCollection<Project> Projects { get; } = new();

        #endregion

        #region Constructors

        public CreateTaskPopupViewModel(
            IRepository<ProjectTask> taskRepository,
            IRepository<Project> projectRepository,
            IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            CurrentUser = _authService.CurrentUser;
            
            CurrentUser = _authService.CurrentUser;
        }

        #endregion

        #region Methods

        public async Task LoadProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            Projects.Clear();
            foreach(var p in projects)
            {
                Projects.Add(p);
            }
        }

        public void SetDate(DateTime date)
        {
            DueDate = date;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Create()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var newTask = new ProjectTask
            {
                Name = Name,
                Type = SelectedType,
                ProjectId = SelectedProject?.Id ?? Guid.Empty,
                FinishDate = DueDate,
                StartDate = DueDate // Default start to due date for now
            };

            await _taskRepository.AddAsync(newTask);
            TaskCreated?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
