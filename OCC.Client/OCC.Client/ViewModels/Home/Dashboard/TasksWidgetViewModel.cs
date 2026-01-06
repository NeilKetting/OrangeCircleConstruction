using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Linq;
using OCC.Client.ViewModels.Home.Shared;
using System;
using CommunityToolkit.Mvvm.Messaging;

namespace OCC.Client.ViewModels.Home.Dashboard
{
    public partial class TasksWidgetViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<HomeTaskItem> _tasks = new();

        #endregion

        #region Constructors

        public TasksWidgetViewModel()
        {
            // Parameterless constructor for design-time support
        }
        
        public TasksWidgetViewModel(IRepository<ProjectTask> taskRepository)
        {
            _taskRepository = taskRepository;
            LoadTasks();
            
            // Subscribe to updates
            WeakReferenceMessenger.Default.Register<Messages.TaskUpdatedMessage>(this, (r, m) => LoadTasks());
        }

        #endregion

        #region Methods

        private async void LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var recentTasks = tasks.OrderByDescending(t => t.StartDate).Take(5); // Show recent or upcoming

            Tasks.Clear();
            foreach (var task in recentTasks)
            {
                Tasks.Add(new HomeTaskItem
                {
                    Id = task.Id,
                    Title = task.Name,
                    Description = task.Description,
                    DueDate = task.FinishDate,
                    Status = task.Status,
                    Priority = task.Priority,
                    AssigneeInitials = string.IsNullOrEmpty(task.AssignedTo) ? "UN" : task.AssignedTo.Substring(0, Math.Min(2, task.AssignedTo.Length)).ToUpper()
                });
            }
        }

        #endregion
    }
}
