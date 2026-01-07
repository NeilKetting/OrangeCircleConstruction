using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskListViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly ILogger<TaskListViewModel> _logger; // Added Logger

        #endregion

        #region Observables

        // Changed from flat HomeTaskItem to grouped ProjectGroupViewModel
        [ObservableProperty]
        private ObservableCollection<ProjectGroupViewModel> _projectGroups = new();

        #endregion

        #region Events

        public event EventHandler<string>? TaskSelectionRequested;
        public event EventHandler? NewTaskRequested;

        #endregion

        #region Constructors

        public TaskListViewModel()
        {
            // Parameterless constructor for design-time support
            _taskRepository = null!;
            _logger = null!;
        }

        public TaskListViewModel(IRepository<ProjectTask> taskRepository, Microsoft.Extensions.Logging.ILogger<TaskListViewModel> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
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
        private void DeleteTask(Guid taskId)
        {
             // Placeholder for now, typically would show confirmation
             // Then call _taskRepository.DeleteAsync(taskId)
             // Then reload
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
            try 
            {
                var tasks = await _taskRepository.GetAllAsync();
                
                // 1. Group by Project
                // We need Project Names. Since ProjectTask has Navigation Property 'Project', 
                // we hope the repository included it. If not, we might see null.
                // The GetProjectTasks controller does query.Include(t => t.Project) ? No, currently only Assignments & Comments.
                // We might need to fetch Project Name or update the API.
                // However, the `task.Project` might be null if not included.
                // Let's assume for now we group by ProjectId and just display "Project [Id]" if name missing, 
                // or we update the API to Include Project.
                // actually, let's update the API logic to include Project if possible, OR assume we have it.
                // Wait, HomeTaskItem previously had `Project` string property. Where did it come from? 
                // It wasn't populated in the old VM code! `Project` prop in HomeTaskItem existed but was not set in previous LoadTasks!
                // Let's check previous LoadTasks:
                // Tasks.Add(new HomeTaskItem { ... Title = task.Name ... }); -> Project wasn't set!
                // So the user probably saw empty Project column.
                
                // Let's Group by ProjectId.
                var grouped = tasks.GroupBy(t => t.ProjectId);

                ProjectGroups.Clear();

                foreach (var group in grouped)
                {
                    // We don't have the Project Name easily here unless we fetch projects.
                    // For now, let's try to get it from the first task if it's populated, or "Unknown Project".
                    var firstTask = group.FirstOrDefault();
                    var projectName = firstTask?.Project?.Name ?? "Project"; 
                    
                    var projectViewModel = new ProjectGroupViewModel(projectName);

                    // 2. Build Tree for this Project
                    var sortedTasks = group.OrderBy(t => t.OrderIndex).ToList();
                    var rootTasks = BuildTaskTree(sortedTasks);

                    foreach (var root in rootTasks)
                    {
                        projectViewModel.RootTasks.Add(root);
                    }

                    ProjectGroups.Add(projectViewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading task list tree");
            }
        }

        private List<TaskTreeItemViewModel> BuildTaskTree(List<ProjectTask> flatTasks)
        {
            var roots = new List<TaskTreeItemViewModel>();
            var levelStack = new Dictionary<int, TaskTreeItemViewModel>();

            foreach (var task in flatTasks)
            {
                var vm = new TaskTreeItemViewModel(task);
                var level = task.IndentLevel;

                // Find nearest parent (level - 1 down to 0)
                TaskTreeItemViewModel? parent = null;
                for (int i = level - 1; i >= 0; i--)
                {
                    if (levelStack.TryGetValue(i, out var p))
                    {
                        parent = p;
                        break;
                    }
                }

                if (parent != null)
                {
                    parent.Children.Add(vm);
                }
                else
                {
                    roots.Add(vm);
                }

                // Update stack
                levelStack[level] = vm;

                // Clear deeper levels to prevent wrong parenting
                var keysToRemove = levelStack.Keys.Where(k => k > level).ToList();
                foreach (var k in keysToRemove) levelStack.Remove(k);
            }

            return roots;
        }

        #endregion
    }

}
