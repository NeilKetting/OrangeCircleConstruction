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

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IRepository<TaskComment> _commentRepository;
        
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private Guid _currentTaskId; 
        private bool _isLoading = false;
        private bool _isUpdatingDuration = false;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _id = "T-1";

        [ObservableProperty]
        private string _title = "Test";

        [ObservableProperty]
        private bool? _isCompleted;

        [ObservableProperty]
        private string _description = "Add a description";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SubtaskCount))]
        private ObservableCollection<ChecklistItem> _toDoList = new ObservableCollection<ChecklistItem>();

        [ObservableProperty]
        private ObservableCollection<string> _tags = new ObservableCollection<string>();

        [ObservableProperty]
        private string _newToDoContent = string.Empty;

        [ObservableProperty]
        private DateTime? _plannedStartDate; 

        [ObservableProperty]
        private DateTime? _dueDate;

        [ObservableProperty]
        private string _plannedDuration = "None"; 

        [ObservableProperty]
        private double? _plannedHours;

        [ObservableProperty]
        private string _plannedCost = "R200";

        [ObservableProperty]
        private DateTime? _actualStartDate;

        [ObservableProperty]
        private DateTime? _doneDate;

        [ObservableProperty]
        private string _actualDuration = "None";

        [ObservableProperty]
        private double? _actualHours;

        [ObservableProperty]
        private string _actualCost = "None";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CommentsCount))]
        private ObservableCollection<TaskComment> _comments = new();

        [ObservableProperty]
        private string _newCommentContent = string.Empty;

        [ObservableProperty]
        private string _status = "Not Started";

        [ObservableProperty]
        private double _progressPercent = 0;

        [ObservableProperty]
        private bool _isOnHold;

        [ObservableProperty] 
        private string _priority = "Medium";

        [ObservableProperty]
        private string _statusColor = "#CBD5E1"; 

        [ObservableProperty]
        private int _percentComplete;

        [ObservableProperty]
        private ObservableCollection<TaskAssignment> _assignments = new();

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _subtasks = new();

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _visibleSubtasks = new();

        [ObservableProperty]
        private bool _hasMoreSubtasks;

        [ObservableProperty]
        private bool _isShowingAllSubtasks;

        #endregion

        #region Properties

        public int SubtaskCount => ToDoList?.Count ?? 0;
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

        public TaskDetailViewModel()
        {
            // Parameterless constructor for design-time support
             _projectTaskRepository = null!;
             _staffRepository = null!;
             _assignmentRepository = null!;
             _commentRepository = null!;
        }

        public TaskDetailViewModel(
            IRepository<ProjectTask> projectTaskRepository,
            IRepository<Employee> staffRepository,
            IRepository<TaskAssignment> assignmentRepository,
            IRepository<TaskComment> commentRepository)
        {
            _projectTaskRepository = projectTaskRepository;
            _staffRepository = staffRepository;
            _assignmentRepository = assignmentRepository;
            _commentRepository = commentRepository;
        }

        #endregion

        #region Commands

        [RelayCommand]
        public void SetStatus(string status)
        {
            Status = status;
        }

        [RelayCommand]
        public void ToggleOnHold()
        {
            IsOnHold = !IsOnHold;
        }

        [RelayCommand]
        public void SetPriority(string priority)
        {
            Priority = priority;
            // OnPriorityChanged handles update
        }

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

        [RelayCommand]
        public void CommitDurations()
        {
            FormatPlannedDuration(PlannedDuration);
            FormatActualDuration(ActualDuration);
        }

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

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ShowAllSubtasks()
        {
            IsShowingAllSubtasks = true;
            UpdateVisibleSubtasks();
        }

        [RelayCommand]
        private void OpenSubtask(ProjectTask subtask)
        {
             // For now, load this subtask in the current view? 
             // Or request navigation. Ideally request navigation.
             // We can re-use LoadTaskById logic effectively "drilling down".
             if (subtask != null)
             {
                 LoadTaskById(subtask.Id);
             }
        }

        [RelayCommand]
        private void AddSubtask()
        {
            // Placeholder: Add a new dummy subtask for now, or logic to create real one
             var newSubtask = new ProjectTask
             {
                 Name = "New Subtask",
                 ProjectId = Guid.Empty, // Should link to parent's project
                 // ParentId? Model doesn't have it explicit but IndentLevel/Order implies it.
                 // For this simple view, we just add it to Children.
                 IndentLevel = 1 // Simplified
             };
             
             Subtasks.Add(newSubtask);
             UpdateVisibleSubtasks();
        }

        #endregion

        #region Methods

        public async void LoadTaskById(Guid taskId)
        {
            _currentTaskId = taskId;
            var task = await _projectTaskRepository.GetByIdAsync(taskId);
            if (task != null) LoadTask(task);
            
            await LoadAssignableResources();
        }

        private async Task LoadAssignableResources()
        {
            AvailableStaff.Clear();
            var staff = await _staffRepository.GetAllAsync();
            foreach(var s in staff) AvailableStaff.Add(s);
            
            await LoadComments();
            await LoadAssignments();
        }

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

        private async Task LoadAssignments()
        {
             Assignments.Clear();
             var assignments = await _assignmentRepository.FindAsync(a => a.ProjectTaskId == _currentTaskId);
             foreach(var assign in assignments)
             {
                 Assignments.Add(assign);
             }
        }

        private void LoadTask(ProjectTask task)
        {
            _isLoading = true;
            try
            {
                Id = $"T-{task.Id.ToString().Substring(task.Id.ToString().Length - 4)}"; 
                Title = task.Name;
                Description = task.Description;
                IsCompleted = task.IsComplete;
                Status = task.Status;
                PercentComplete = task.PercentComplete;
                ProgressPercent = task.PercentComplete;
                IsOnHold = task.IsOnHold;
                Priority = task.Priority;

                // Map Dates
                PlannedStartDate = task.StartDate == DateTime.MinValue ? null : task.StartDate;
                DueDate = task.FinishDate == DateTime.MinValue ? null : task.FinishDate;
                ActualStartDate = task.ActualStartDate;
                DoneDate = task.ActualCompleteDate;

                // Calculations
                PlannedHours = task.PlanedDurationHours?.TotalHours ?? CalculatePlannedHours(task);
                ActualHours = task.ActualDuration?.TotalHours;

                UpdatePlannedDuration();
                UpdateActualDuration();

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

        public async void UpdateTask()
        {
            if (_isLoading) return;
            if (_currentTaskId == Guid.Empty) return;

            // Wait for lock to prevent threading issues on DbContext
            await _updateLock.WaitAsync();
            try
            {
                var task = await _projectTaskRepository.GetByIdAsync(_currentTaskId);
                if (task == null) return;

                // Update fields from properties
                task.Name = Title;
                task.Description = Description;
                
                task.StartDate = PlannedStartDate ?? DateTime.Now; 
                task.FinishDate = DueDate ?? DateTime.Now;
                task.ActualStartDate = ActualStartDate;
                task.ActualCompleteDate = DoneDate;
                
                task.Status = Status;
                task.PercentComplete = (int)ProgressPercent;
                task.IsOnHold = IsOnHold;
                task.Priority = Priority;

                if (PlannedHours.HasValue)
                    task.PlanedDurationHours = TimeSpan.FromHours(PlannedHours.Value);
                else
                    task.PlanedDurationHours = null;

                if (ActualHours.HasValue)
                    task.ActualDuration = TimeSpan.FromHours(ActualHours.Value);
                else
                    task.ActualDuration = null;

                await _projectTaskRepository.UpdateAsync(task);
                
                // Notify listeners
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new OCC.Client.ViewModels.Messages.TaskUpdatedMessage(_currentTaskId));
            }
            finally
            {
                _updateLock.Release();
            }
        }

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

        #endregion

        #region Helper Methods

        private double CalculatePlannedHours(ProjectTask task)
        {
             // Fallback calculation
             var days = (task.FinishDate - task.StartDate).TotalDays + 1;
             return Math.Round(days * 8, 1);
        }

        async partial void OnStatusChanged(string value)
        {
            // Auto-update progress
            switch(value)
            {
                case "Not Started": 
                    ProgressPercent = 0; 
                    StatusColor = "#CBD5E1"; // Gray
                    break;
                case "Started": 
                case "Halfway": 
                case "Almost Done": 
                    ProgressPercent = value == "Started" ? 25 : (value == "Halfway" ? 50 : 75); 
                    StatusColor = "#EF4444"; // Red
                    break;
                case "Done": 
                    ProgressPercent = 100; 
                    IsCompleted = true; 
                    StatusColor = "#EF4444"; // Red
                    break;
            }
            if (value != "Done" && IsCompleted == true) IsCompleted = false;

            if (IsOnHold) StatusColor = "#22C55E"; // Green

            await Task.Run(() => UpdateTask());
        }

        async partial void OnIsOnHoldChanged(bool value)
        {
             if (value) StatusColor = "#22C55E"; // Green
             else 
             {
                 switch(Status)
                 {
                    case "Not Started": StatusColor = "#CBD5E1"; break;
                    default: StatusColor = "#EF4444"; break;
                 }
             }
             await Task.Run(() => UpdateTask());
        }

        async partial void OnPriorityChanged(string value) => await Task.Run(() => UpdateTask());

        async partial void OnPlannedStartDateChanged(DateTime? value) 
        {
            if (PlannedStartDate.HasValue && DueDate.HasValue)
                 PlannedHours = CalculatePlannedHours(new ProjectTask { StartDate = PlannedStartDate.Value, FinishDate = DueDate.Value });
            UpdatePlannedDuration();
            await Task.Run(() => UpdateTask());
        }

        async partial void OnDueDateChanged(DateTime? value) 
        {
            if (PlannedStartDate.HasValue && DueDate.HasValue)
                 PlannedHours = CalculatePlannedHours(new ProjectTask { StartDate = PlannedStartDate.Value, FinishDate = DueDate.Value });
            UpdatePlannedDuration();
            await Task.Run(() => UpdateTask());
        }

        async partial void OnPlannedHoursChanged(double? value) 
        {
            if (_isUpdatingDuration) return;
            
            // Sync Duration Text
            if (value.HasValue)
            {
                _isUpdatingDuration = true;
                double days = value.Value / 8.0;
                PlannedDuration = $"{days} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
            
            await Task.Run(() => UpdateTask());
        }

        async partial void OnActualStartDateChanged(DateTime? value) 
        {
            UpdateActualDuration();
            await Task.Run(() => UpdateTask());
        }

        async partial void OnDoneDateChanged(DateTime? value) 
        {
            UpdateActualDuration();
            await Task.Run(() => UpdateTask());
        }

        async partial void OnPlannedDurationChanged(string value)
        {
            if (_isUpdatingDuration) return;

             // Try parse days
             FormatPlannedDuration(value);

            await Task.Run(() => UpdateTask());
        }

        async partial void OnActualDurationChanged(string value)
        {
            if (_isUpdatingDuration) return;
            await Task.Run(() => UpdateTask());
        }

        private void FormatPlannedDuration(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "None") return;
            
            var numericPart = "";
            bool foundDecimal = false;
            foreach (char c in value)
            {
                if (char.IsDigit(c)) numericPart += c;
                else if (c == '.' && !foundDecimal) { numericPart += c; foundDecimal = true; }
                else if (numericPart.Length > 0) break;
            }

            if (double.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double days))
            {
                _isUpdatingDuration = true;
                PlannedDuration = $"{days} {(days == 1 ? "day" : "days")}";
                
                // Sync Hours
                PlannedHours = days * 8.0;
                
                _isUpdatingDuration = false;
            }
        }

        private void FormatActualDuration(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "None") return;

            var numericPart = "";
            bool foundDecimal = false;
            foreach (char c in value)
            {
                if (char.IsDigit(c)) numericPart += c;
                else if (c == '.' && !foundDecimal) { numericPart += c; foundDecimal = true; }
                else if (numericPart.Length > 0) break;
            }

            if (double.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double days))
            {
                _isUpdatingDuration = true;
                ActualDuration = $"{days} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
        }

        private void UpdatePlannedDuration()
        {
            if (PlannedStartDate.HasValue && DueDate.HasValue)
            {
                var days = (DueDate.Value.Date - PlannedStartDate.Value.Date).TotalDays + 1;
                _isUpdatingDuration = true;
                PlannedDuration = $"{days} {(days == 1 ? "day" : "days")}";
                _isUpdatingDuration = false;
            }
            else
            {
                _isUpdatingDuration = true;
                PlannedDuration = "None";
                _isUpdatingDuration = false;
            }
        }

        private void UpdateActualDuration()
        {
            if (ActualStartDate.HasValue && DoneDate.HasValue)
            {
                var days = (DoneDate.Value.Date - ActualStartDate.Value.Date).TotalDays + 1;
                ActualDuration = $"{days} {(days == 1 ? "day" : "days")}";
            }
            else
            {
                ActualDuration = "None";
            }
        }

        async partial void OnActualHoursChanged(double? value) => await Task.Run(() => UpdateTask());
        async partial void OnDescriptionChanged(string value) => await Task.Run(() => UpdateTask());
        async partial void OnTitleChanged(string value) => await Task.Run(() => UpdateTask());
        async partial void OnIsCompletedChanged(bool? value) 
        {
             // Special handling for Done Checkbox (could set DoneDate automatically)
             if(value == true && DoneDate == null) DoneDate = DateTime.Now;
             if(value == false) DoneDate = null;
             
             await Task.Run(() => UpdateTask());
        }

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
