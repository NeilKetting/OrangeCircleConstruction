using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskListViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<HomeTaskItem> _tasks = new();

        #endregion

        #region Events

        public event EventHandler<string>? TaskSelectionRequested;
        public event EventHandler? NewTaskRequested;

        #endregion

        #region Constructors

        public TaskListViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public TaskListViewModel(IRepository<ProjectTask> taskRepository)
        {
            _taskRepository = taskRepository;
            LoadTasks();

            // Subscribe to updates
            WeakReferenceMessenger.Default.Register<Messages.TaskUpdatedMessage>(this, (r, m) =>
            {
                LoadTasks();
            });
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void SelectTask(Guid taskId)
        {
            TaskSelectionRequested?.Invoke(this, taskId.ToString());
        }

        [RelayCommand]
        public void NewTask()
        {
            NewTaskRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public async void LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(new HomeTaskItem
                {
                    Id = task.Id, 
                    Title = task.Name,
                    Description = task.Description,
                    DueDate = task.FinishDate, 
                    Status = task.Status, 
                    Priority = task.Priority,
                    AssigneeInitials = task.AssignedTo.Substring(0, Math.Min(2, task.AssignedTo.Length)).ToUpper()
                });
            }
        }

        #endregion
    }
}
