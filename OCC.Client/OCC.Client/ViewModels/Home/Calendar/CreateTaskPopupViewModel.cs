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
        private readonly IRepository<TaskItem> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;

        public event EventHandler? CloseRequested;
        public event EventHandler? TaskCreated;

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

        public ObservableCollection<TaskType> TaskTypes { get; } = new ObservableCollection<TaskType>(Enum.GetValues<TaskType>());
        public ObservableCollection<Project> Projects { get; } = new();

        public CreateTaskPopupViewModel(
            IRepository<TaskItem> taskRepository,
            IRepository<Project> projectRepository,
            IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            CurrentUser = _authService.CurrentUser;
            
            LoadProjects();
        }

        private async void LoadProjects()
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

        [RelayCommand]
        private async Task Create()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var newTask = new TaskItem
            {
                Name = Name,
                Type = SelectedType,
                ProjectId = SelectedProject?.Id,
                PlanedDueDate = DueDate,
                PlanedStartDate = DueDate // Default start to due date for now
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
    }
}
