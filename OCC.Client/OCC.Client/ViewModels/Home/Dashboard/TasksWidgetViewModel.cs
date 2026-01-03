using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Linq;
using OCC.Client.ViewModels.Home.Shared;

namespace OCC.Client.ViewModels.Home.Dashboard
{
    public partial class TasksWidgetViewModel : ViewModelBase
    {
        private readonly IRepository<TaskItem> _taskRepository;

        [ObservableProperty]
        private ObservableCollection<HomeTaskItem> _tasks = new();

        public TasksWidgetViewModel(IRepository<TaskItem> taskRepository)
        {
            _taskRepository = taskRepository;
            LoadTasks();
        }

        private async void LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            Tasks.Clear();
            // Take first 3 for summary view or similar logic
            foreach (var task in tasks.Take(3))
            {
                Tasks.Add(new HomeTaskItem
                {
                    Id = task.Id,
                    IsCompleted = false,
                    TaskName = task.Name,
                    Project = "Project", // Placeholder, would fetch project name in real scenario
                    Progress = "0%",
                    Priority = "High",
                    Due = task.PlanedDueDate?.ToString("MMM dd") ?? "None"
                });
            }
        }
    }
}
