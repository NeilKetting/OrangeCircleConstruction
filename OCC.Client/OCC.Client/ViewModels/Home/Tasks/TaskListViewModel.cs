using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Shared;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<HomeTaskItem> _tasks = new();

        public event EventHandler<string>? TaskSelectionRequested;

        private readonly IRepository<TaskItem> _taskRepository;

        public TaskListViewModel(IRepository<TaskItem> taskRepository)
        {
            _taskRepository = taskRepository;
            LoadTasks();
        }

        private async void LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(new HomeTaskItem
                {
                    Id = task.Id,
                    IsCompleted = false, // Mock default
                    TaskName = task.Name,
                    Project = "Construction Project", // Static for now until Project repo is linked
                    Progress = "0%",
                    Priority = "Medium",
                    Due = task.PlanedDueDate?.ToString("MMM dd") ?? "None"
                });
            }
        }

        [RelayCommand]
        private void SelectTask(Guid taskId)
        {
            TaskSelectionRequested?.Invoke(this, taskId.ToString());
        }
    }
}
