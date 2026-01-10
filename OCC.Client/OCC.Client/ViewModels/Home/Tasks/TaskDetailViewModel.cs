using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Threading.Tasks;
using System.Threading;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ModelWrappers;

namespace OCC.Client.ViewModels.Home.Tasks
{
    /// <summary>
    /// ViewModel for displaying and editing details of a specific ProjectTask.
    /// This ViewModel uses the Model Wrapper pattern (<see cref="ProjectTaskWrapper"/>) to 
    /// separate presentation logic from the data model and ensure clean, reactive bindings.
    /// It manages subtasks, assignments, comments, and orchestrates data loading and saving via repositories.
    /// </summary>
    public partial class TaskDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IRepository<TaskComment> _commentRepository;
        private readonly IDialogService _dialogService;
        
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private Guid _currentTaskId; 
        private bool _isLoading = false;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;

        #endregion

        #region Observables

        /// <summary>
        /// The wrapper around the current ProjectTask, providing reactive properties for the UI.
        /// </summary>
        [ObservableProperty]
        private ProjectTaskWrapper _task;

        /// <summary>
        /// Collection of checklist items (To-Do list) associated with the task.
        /// Changes to this collection notify the SubtaskCount property.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubtaskCount))]
        private ObservableCollection<ChecklistItem> _toDoList = new ObservableCollection<ChecklistItem>();

        /// <summary>
        /// Collection of tags or labels applied to the task.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _tags = new ObservableCollection<string>();

        /// <summary>
        /// Helper property for the input field when adding a new checklist item.
        /// </summary>
        [ObservableProperty]
        private string _newToDoContent = string.Empty;

        /// <summary>
        /// Collection of comments posted primarily on this task.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CommentsCount))]
        private ObservableCollection<TaskComment> _comments = new();

        /// <summary>
        /// Helper property for the input field when adding a new comment.
        /// </summary>
        [ObservableProperty]
        private string _newCommentContent = string.Empty;

        /// <summary>
        /// The list of users assigned to this task.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<TaskAssignment> _assignments = new();

        /// <summary>
        /// The complete list of subtasks (children) for this task.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ProjectTask> _subtasks = new();

        /// <summary>
        /// The subset of subtasks currently visible in the UI (handles pagination/preview limits).
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ProjectTask> _visibleSubtasks = new();

        /// <summary>
        /// Indicates if there are more subtasks than currently displayed.
        /// </summary>
        [ObservableProperty]
        private bool _hasMoreSubtasks;

        /// <summary>
        /// Toggles whether to show the full list of subtasks or just a preview.
        /// </summary>
        [ObservableProperty]
        private bool _isShowingAllSubtasks;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the total count of items in the To-Do list.
        /// </summary>
        public int SubtaskCount => ToDoList?.Count ?? 0;

        /// <summary>
        /// Gets the total count of comments on this task.
        /// </summary>
        public int CommentsCount => Comments?.Count ?? 0;

        public int AttachmentsCount => 0; // Placeholder for now

        public ObservableCollection<string> PriorityLevels { get; } = new() 
        { 
            "Critical", "Very High", "High", "Medium", "Low", "Very Low" 
        };

        // Lists for selection
        public ObservableCollection<Employee> AvailableStaff { get; } = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetailViewModel"/> class.
        /// Parameterless constructor required for design-time support.
        /// </summary>
        public TaskDetailViewModel()
        {
             _projectTaskRepository = null!;
             _staffRepository = null!;
             _assignmentRepository = null!;
             _commentRepository = null!;
             _dialogService = null!;
             _task = new ProjectTaskWrapper(new ProjectTask());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetailViewModel"/> class with required dependencies.
        /// </summary>
        /// <param name="projectTaskRepository">Repository for accessing ProjectTask data.</param>
        /// <param name="staffRepository">Repository for accessing Employee data.</param>
        /// <param name="assignmentRepository">Repository for managing task assignments.</param>
        /// <param name="commentRepository">Repository for managing task comments.</param>
        /// <param name="dialogService">Service for showing alerts.</param>
        public TaskDetailViewModel(
            IRepository<ProjectTask> projectTaskRepository,
            IRepository<Employee> staffRepository,
            IRepository<TaskAssignment> assignmentRepository,
            IRepository<TaskComment> commentRepository,
            IDialogService dialogService)
        {
            _projectTaskRepository = projectTaskRepository;
            _staffRepository = staffRepository;
            _assignmentRepository = assignmentRepository;
            _commentRepository = commentRepository;
            _dialogService = dialogService;
            _task = new ProjectTaskWrapper(new ProjectTask());
        }

        #endregion

        #region Commands

        /// <summary>
        /// Updates the status of the current task.
        /// </summary>
        /// <param name="status">The new status string (e.g. "InProgress", "Done").</param>
        [RelayCommand]
        public void SetStatus(string status)
        {
            Task.Status = status;
        }

        /// <summary>
        /// Toggles the 'On Hold' state of the task, updating the status color accordingly.
        /// </summary>
        [RelayCommand]
        public void ToggleOnHold()
        {
            Task.IsOnHold = !Task.IsOnHold;
        }

        /// <summary>
        /// Updates the priority level of the task.
        /// </summary>
        /// <param name="priority">The new priority level (e.g. "High", "Low").</param>
        [RelayCommand]
        public void SetPriority(string priority)
        {
            Task.Priority = priority;
        }

        /// <summary>
        /// Assigns a staff member to the current task.
        /// Prevents duplicate assignments for the same staff member.
        /// </summary>
        /// <param name="staff">The employee to assign.</param>
        [RelayCommand]
        public async System.Threading.Tasks.Task AssignStaff(Employee staff)
        {
            if (staff == null || Assignments.Any(a => a.AssigneeId == staff.Id)) return;
            
            await _updateLock.WaitAsync();
            try
            {
                var assignment = new TaskAssignment
                {
                     ProjectTaskId = _currentTaskId,
                     AssigneeId = staff.Id,
                     AssigneeType = AssigneeType.Staff,
                     AssigneeName = $"{staff.FirstName} {staff.LastName}"
                };
                
                await _assignmentRepository.AddAsync(assignment);
                Assignments.Add(assignment);
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Removes an existing assignment from the task.
        /// </summary>
        /// <param name="assignment">The assignment record to remove.</param>
        [RelayCommand]
        public async System.Threading.Tasks.Task RemoveAssignment(TaskAssignment assignment)
        {
            if (assignment == null) return;
            
            await _updateLock.WaitAsync();
            try
            {
                await _assignmentRepository.DeleteAsync(assignment.Id);
                Assignments.Remove(assignment);
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Commits planned and actual duration changes to the model.
        /// Triggered manually or by loss of focus on duration fields.
        /// </summary>
        [RelayCommand]
        public void CommitDurations()
        {
           // Handled by Wrapper logic via TwoWay binding
        }

        /// <summary>
        /// Adds a new item to the To-Do checklist based on the NewToDoContent property.
        /// </summary>
        [RelayCommand]
        private void AddToDo()
        {
            if (!string.IsNullOrWhiteSpace(NewToDoContent))
            {
                ToDoList.Add(new ChecklistItem(NewToDoContent));
                NewToDoContent = string.Empty;
                OnPropertyChanged(nameof(SubtaskCount));
            }
        }

        /// <summary>
        /// Adds a new comment to the task and saves it to the repository.
        /// </summary>
        [RelayCommand]
        private void AddComment()
        {
            if (!string.IsNullOrWhiteSpace(NewCommentContent))
            {
                var newComment = new TaskComment
                {
                    AuthorName = "Current User", // In real app, get from AuthService
                    AuthorEmail = "user@occ.com",
                    Content = NewCommentContent,
                    CreatedAt = DateTime.Now
                };

                Comments.Insert(0, newComment);
                NewCommentContent = string.Empty;
                OnPropertyChanged(nameof(CommentsCount));

                // Save to Task
                SaveCommentToTask(newComment);
            }
        }

        /// <summary>
        /// Closes the Task Detail view.
        /// Raises the CloseRequested event and cleans up subscriptions.
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            // Clean up subscription
            if(Task != null) Task.PropertyChanged -= Task_PropertyChanged;
        }

        /// <summary>
        /// Expands the subtask list to show all children, instead of just the preview limit.
        /// </summary>
        [RelayCommand]
        private void ShowAllSubtasks()
        {
            IsShowingAllSubtasks = true;
            UpdateVisibleSubtasks();
        }

        /// <summary>
        /// Navigates to or loads the details of a specific subtask.
        /// </summary>
        /// <param name="subtask">The subtask to open.</param>
        [RelayCommand]
        private void OpenSubtask(ProjectTask subtask)
        {
             if (subtask != null)
             {
                 LoadTaskById(subtask.Id);
             }
        }

        /// <summary>
        /// Creates a new child task (subtask) under the current task.
        /// Note: This currently creates an in-memory subtask. 
        /// In a full implementation, this should persist the new subtask to the repository.
        /// </summary>
        [RelayCommand]
        private void AddSubtask()
        {
             var newSubtask = new ProjectTask
             {
                 Name = "New Subtask",
                 ProjectId = Guid.Empty, 
                 IndentLevel = 1 
             };
             
             Subtasks.Add(newSubtask);
             UpdateVisibleSubtasks();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads a task by its ID from the repository and initializes all related resources.
        /// </summary>
        /// <param name="taskId">The GUID of the task to load.</param>
        public async void LoadTaskById(Guid taskId)
        {
            try 
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Loading Task: {taskId}...");
                _currentTaskId = taskId;
                var task = await _projectTaskRepository.GetByIdAsync(taskId);
                if (task != null) LoadTask(task);
                
                await LoadAssignableResources();
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] LoadTaskById Complete.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] CRASH in LoadTaskById: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Stack: {ex.StackTrace}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading task details: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads available staff, comments, and current assignments for the task.
        /// </summary>
        private async Task LoadAssignableResources()
        {
            try 
            {
                AvailableStaff.Clear();
                var staff = await _staffRepository.GetAllAsync();
                foreach(var s in staff) AvailableStaff.Add(s);
                
                await LoadComments();
                await LoadAssignments();
            }
            catch (Exception ex)
            {
                // Catch secondary load errors
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Error loading resources: {ex.Message}");
                throw; // propagate to parent catch
            }
        }

        /// <summary>
        /// Fetches comments associated with the current task and populates the Comments collection.
        /// </summary>
        private async Task LoadComments()
        {
             Comments.Clear();
             var comments = await _commentRepository.FindAsync(c => c.TaskId == _currentTaskId);
             
             foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
             {
                 Comments.Add(comment);
             }
             OnPropertyChanged(nameof(CommentsCount));
        }

        /// <summary>
        /// Fetches assignments for the current task and populates the Assignments collection.
        /// </summary>
        private async Task LoadAssignments()
        {
             Assignments.Clear();
             var assignments = await _assignmentRepository.FindAsync(a => a.ProjectTaskId == _currentTaskId);
             foreach(var assign in assignments)
             {
                 Assignments.Add(assign);
             }
        }

        /// <summary>
        /// Initializes the ViewModel with a specific ProjectTask instance.
        /// Sets up the wrapper, subscriptions, and subtask visibility.
        /// </summary>
        /// <param name="task">The ProjectTask model to display.</param>
        private void LoadTask(ProjectTask task)
        {
            _isLoading = true;
            try
            {
                // Unsubscribe previous
                if (Task != null) Task.PropertyChanged -= Task_PropertyChanged;

                Task = new ProjectTaskWrapper(task);
                Task.PropertyChanged += Task_PropertyChanged;

                // Load Subtasks
                Subtasks.Clear();
                if (task.Children != null)
                {
                    foreach(var child in task.Children) Subtasks.Add(child);
                }
                UpdateVisibleSubtasks();

                OnPropertyChanged(nameof(SubtaskCount));
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Event handler for property changes on the task wrapper.
        /// Triggers an async update to the data model.
        /// </summary>
        private async void Task_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isLoading) return;
            await UpdateTask();
        }

        /// <summary>
        /// Persists changes from the wrapper back to the data model/database.
        /// Uses a semaphore to ensure serial access and sends a TaskUpdatedMessage upon success.
        /// </summary>
        public async System.Threading.Tasks.Task UpdateTask()
        {
            if (_isLoading) return;
            if (_currentTaskId == Guid.Empty) return;

            await _updateLock.WaitAsync();
            try
            {
                // Sync Wrapper back to Model
                Task.CommitToModel();

                // Save to DB
                // Note: Task.Model is the same reference we passed in, so we just save it.
                await _projectTaskRepository.UpdateAsync(Task.Model);
                
                // Notify listeners
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new OCC.Client.ViewModels.Messages.TaskUpdatedMessage(_currentTaskId));
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Saves a new comment to the repository.
        /// </summary>
        /// <param name="comment">The TaskComment to save.</param>
        private async void SaveCommentToTask(TaskComment comment)
        {
            if (_currentTaskId == Guid.Empty) return;

            await _updateLock.WaitAsync();
            try
            {
                comment.TaskId = _currentTaskId;
                await _commentRepository.AddAsync(comment);
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Updates the VisibleSubtasks collection based on the 'Show All' toggle and preview limit.
        /// </summary>
        private void UpdateVisibleSubtasks()
        {
            VisibleSubtasks.Clear();
            if (IsShowingAllSubtasks)
            {
                foreach(var s in Subtasks) VisibleSubtasks.Add(s);
                HasMoreSubtasks = false;
            }
            else
            {
                var take = 5;
                foreach(var s in Subtasks.Take(take)) VisibleSubtasks.Add(s);
                HasMoreSubtasks = Subtasks.Count > take;
            }
        }

        #endregion
    }
}
