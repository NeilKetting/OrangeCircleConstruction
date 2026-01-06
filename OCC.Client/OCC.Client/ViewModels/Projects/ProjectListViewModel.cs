using Avalonia.Threading;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Shared.Models;
using System;
using OCC.Client.Services;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectListViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;
        private readonly System.Threading.SemaphoreSlim _loadLock = new(1, 1);

        #endregion

        #region Events

        public event EventHandler<Guid>? TaskSelectionRequested;
        public event EventHandler? NewTaskRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _tasks = new();

        [ObservableProperty]
        private ProjectTask? _selectedTask;

        [ObservableProperty]
        private Guid _currentProjectId;

        #endregion

        #region Properties

        public bool HasTasks => Tasks.Count > 0;

        #endregion

        #region Constructors

        public ProjectListViewModel()
        {
            // Parameterless constructor for design-time support
        }
        
        public ProjectListViewModel(
            IRepository<ProjectTask> taskRepository,
            IRepository<Project> projectRepository,
            IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            
            // Subscribe to updates
            WeakReferenceMessenger.Default.Register<ViewModels.Messages.TaskUpdatedMessage>(this, (r, m) =>
            {
                if (CurrentProjectId != Guid.Empty)
                {
                    LoadTasks(CurrentProjectId);
                }
            });
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddNewTask()
        {
            if (CurrentProjectId == Guid.Empty) return;
            NewTaskRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public async void LoadTasks(Guid projectId)
        {
            await _loadLock.WaitAsync();
            try
            {
                CurrentProjectId = projectId;
                var tasks = await _taskRepository.FindAsync(t => t.ProjectId == projectId);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Tasks.Clear();
                    foreach (var task in tasks)
                    {
                        Tasks.Add(task);
                    }
                    OnPropertyChanged(nameof(HasTasks));
                });
            }
            finally
            {
                _loadLock.Release();
            }
        }

        partial void OnSelectedTaskChanged(ProjectTask? value)
        {
            if (value != null)
            {
                TaskSelectionRequested?.Invoke(this, value.Id);
                SelectedTask = null; // Reset selection so it can be clicked again
            }
        }

        #endregion
    }
}
