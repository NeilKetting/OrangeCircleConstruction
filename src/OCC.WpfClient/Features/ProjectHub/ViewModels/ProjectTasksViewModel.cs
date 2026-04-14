using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using OCC.WpfClient.Infrastructure;

namespace OCC.WpfClient.Features.ProjectHub.ViewModels
{
    public partial class ProjectTasksViewModel : ViewModelBase
    {
        [ObservableProperty] private ObservableCollection<ProjectTask> _tasks = new();
        [ObservableProperty] private ProjectTask? _selectedTask;
        [ObservableProperty] private bool _hasTasks;

        public ProjectTasksViewModel()
        {
            Title = "Tasks";
        }

        public void UpdateTasks(IEnumerable<ProjectTask> tasks)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Tasks = new ObservableCollection<ProjectTask>(tasks);
                HasTasks = Tasks.Any();
            });
        }

        [RelayCommand]
        private void EditTask(ProjectTask task)
        {
            // Implementation later
        }

        [RelayCommand]
        private void DeleteTask(ProjectTask task)
        {
            // Implementation later
        }
    }
}
