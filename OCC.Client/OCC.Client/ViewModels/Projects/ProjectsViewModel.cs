using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using CommunityToolkit.Mvvm.Input;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectsViewModel : ViewModelBase
    {
        #region Private Members

        private readonly OCC.Client.Services.IRepository<OCC.Shared.Models.Project> _projectRepository;
        private readonly OCC.Client.Services.IRepository<OCC.Shared.Models.ProjectTask> _taskRepository;
        private readonly OCC.Client.Services.IRepository<OCC.Shared.Models.Employee> _staffRepository;

        private readonly OCC.Client.Services.IRepository<OCC.Shared.Models.TaskAssignment> _assignmentRepository;
        private readonly OCC.Client.Services.IRepository<OCC.Shared.Models.TaskComment> _commentRepository;

        #endregion

        #region Observables

        [ObservableProperty]
        private Shared.ProjectTopBarViewModel _topBar;

        [ObservableProperty]
        private ProjectListViewModel _listVM;

        [ObservableProperty]
        private ProjectGanttViewModel _ganttVM;

        [ObservableProperty]
        private Guid _currentProjectId;

        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private bool _isTaskDetailVisible;

        [ObservableProperty]
        private Home.Tasks.TaskDetailViewModel? _currentTaskDetail;

        #endregion

        #region Constructors

        public ProjectsViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public ProjectsViewModel(
            ProjectListViewModel listVM, 
            ProjectGanttViewModel ganttVM,
            OCC.Client.Services.IRepository<OCC.Shared.Models.Project> projectRepository,
            OCC.Client.Services.IRepository<OCC.Shared.Models.ProjectTask> taskRepository,
            OCC.Client.Services.IRepository<OCC.Shared.Models.Employee> staffRepository,
            OCC.Client.Services.IRepository<OCC.Shared.Models.TaskAssignment> assignmentRepository,
            OCC.Client.Services.IRepository<OCC.Shared.Models.TaskComment> commentRepository)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _staffRepository = staffRepository;
            _assignmentRepository = assignmentRepository;
            _commentRepository = commentRepository;
            _topBar = new Shared.ProjectTopBarViewModel();
            _listVM = listVM;
            _ganttVM = ganttVM;
            
            // Default view
            _currentView = _listVM;
            
            // Subscribe to task selection
            _listVM.TaskSelectionRequested += (s, id) => OpenTaskDetail(id);
            _listVM.NewTaskRequested += (s, e) => CreateNewTask();

            _topBar.DeleteProjectRequested += OnDeleteProjectRequested;
            _topBar.PropertyChanged += TopBar_PropertyChanged;

            WeakReferenceMessenger.Default.Register<OCC.Client.ViewModels.Messages.ProjectSelectedMessage>(this, (r, m) =>
            {
               CurrentProjectId = m.Value.Id;
               TopBar.ProjectId = m.Value.Id;
               TopBar.ProjectName = m.Value.Name;
               TopBar.ProjectIconInitials = GetInitials(m.Value.Name);
               
               _listVM.LoadTasks(CurrentProjectId);
               _ganttVM.LoadTasks(CurrentProjectId);
               
               TopBar.ActiveTab = "List";
            });
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void CloseTaskDetail()
        {
            IsTaskDetailVisible = false;
            CurrentTaskDetail = null;
            
            // Refresh list
            ListVM.LoadTasks(CurrentProjectId);
        }

        #endregion

        #region Methods

        private async void OnDeleteProjectRequested(object? sender, EventArgs e)
        {
             if (CurrentProjectId == Guid.Empty) return;
             
             try 
             {
                 await _projectRepository.DeleteAsync(CurrentProjectId);
                 WeakReferenceMessenger.Default.Send(new OCC.Client.ViewModels.Messages.ProjectDeletedMessage(CurrentProjectId));
             }
             catch
             {
                 // Handle error (maybe show toast)
             }
        }
        
        private void TopBar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
             if (e.PropertyName == nameof(Shared.ProjectTopBarViewModel.ActiveTab))
             {
                 switch (TopBar.ActiveTab)
                 {
                     case "List":
                        CurrentView = ListVM;
                        break;
                     case "Gantt":
                        CurrentView = GanttVM;
                        if (CurrentProjectId != Guid.Empty)
                        {
                            // Optional: Refresh if needed
                        }
                        break;
                     default:
                        // Placeholder
                        CurrentView = ListVM; 
                        break;
                 }
             }
        }

        private void OpenTaskDetail(Guid taskId)
        {
            CurrentTaskDetail = new Home.Tasks.TaskDetailViewModel(_taskRepository, _staffRepository, _assignmentRepository, _commentRepository);
            CurrentTaskDetail.LoadTaskById(taskId);
            CurrentTaskDetail.CloseRequested += (s, e) => CloseTaskDetail();
            IsTaskDetailVisible = true;
        }

        private async void CreateNewTask()
        {
            if (CurrentProjectId == Guid.Empty) return;

            var newTask = new OCC.Shared.Models.ProjectTask
            {
                Name = "New Task",
                ProjectId = CurrentProjectId,
                Status = "Not Started",
                Priority = "Medium", 
                Description = ""
            };

            await _taskRepository.AddAsync(newTask);
            
            // Refresh list
            _listVM.LoadTasks(CurrentProjectId);
            
            // Open Details
            OpenTaskDetail(newTask.Id);
        }

        #endregion

        #region Helper Methods

        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "P";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        #endregion
    }
}
