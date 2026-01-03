using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

using OCC.Client.Services;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskDetailViewModel : ViewModelBase
    {
        private Guid _currentTaskId; // Store the Guid

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

        public int SubtaskCount => ToDoList?.Count ?? 0;
        public int CommentsCount => Comments?.Count ?? 0;
        public int AttachmentsCount => 0; // Placeholder for now



        // Data Fields
        [ObservableProperty]
        private DateTime? _plannedStartDate; 

        [ObservableProperty]
        private DateTime? _dueDate;

        [ObservableProperty]
        private string _plannedDuration = "None"; // String to allow "3 days" text

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


        private readonly IRepository<TaskItem> _projectTaskRepository;

        public TaskDetailViewModel(IRepository<TaskItem> projectTaskRepository)
        {
            _projectTaskRepository = projectTaskRepository;
        }

        public async void LoadTaskById(Guid taskId)
        {
            _currentTaskId = taskId;
            var task = await _projectTaskRepository.GetByIdAsync(taskId);
            if (task != null)
            {
                LoadTask(task);
            }
        }

        private void LoadTask(TaskItem task)
        {
            // Format ID as T-1, T-2 etc. Simple numeric extraction for mock, or just T- followed by short Guid/sequential
            Id = $"T-{task.Id.ToString().Substring(0, 1).ToUpper()}"; 
            Title = task.Name;
            Description = task.Description ?? "No description";
            IsCompleted = task.ActualCompleteDate.HasValue;
            
            // Map Dates
            PlannedStartDate = task.PlanedStartDate;
            DueDate = task.PlanedDueDate;
            ActualStartDate = task.ActualStartDate;
            DoneDate = task.ActualCompleteDate;

            // Map Numbers
            if (task.PlanedDurationHours.HasValue)
                PlannedHours = task.PlanedDurationHours.Value.TotalHours;
            else 
                PlannedHours = null;

            if (task.ActualDuration.HasValue)
                ActualHours = task.ActualDuration.Value.TotalHours;
             else
                ActualHours = null;

            // Simple duration string for now (could be calculated if we wanted)
            UpdatePlannedDuration();
            UpdateActualDuration();

            // Populate other fields as needed

            // Mock Comments
            Comments.Clear();
            Comments.Add(new TaskComment
            {
                AuthorName = "Vernon Steenberg",
                AuthorEmail = "projecl@occ.com",
                Content = "444+",
                CreatedAt = DateTime.Now.AddDays(-1).AddHours(-2)
            });
             Comments.Add(new TaskComment
            {
                AuthorName = "Neil Ketting",
                AuthorEmail = "neil@origize63.co.za",
                Content = "Test 1",
                CreatedAt = DateTime.Now.AddDays(-1).AddHours(-4)
            });
             
             OnPropertyChanged(nameof(CommentsCount));
             OnPropertyChanged(nameof(SubtaskCount));
        }


        // Auto-save triggers
        async partial void OnPlannedStartDateChanged(DateTime? value) 
        {
            UpdatePlannedDuration();
            await UpdateTask();
        }

        async partial void OnDueDateChanged(DateTime? value) 
        {
            UpdatePlannedDuration();
            await UpdateTask();
        }

        async partial void OnActualStartDateChanged(DateTime? value) 
        {
            UpdateActualDuration();
            await UpdateTask();
        }

        async partial void OnDoneDateChanged(DateTime? value) 
        {
            UpdateActualDuration();
            await UpdateTask();
        }

        private bool _isUpdatingDuration = false;

        async partial void OnPlannedDurationChanged(string value)
        {
            if (_isUpdatingDuration) return;
            await UpdateTask();
        }

        async partial void OnActualDurationChanged(string value)
        {
            if (_isUpdatingDuration) return;
            await UpdateTask();
        }

        private void FormatPlannedDuration(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "None") return;
            
            // Try to extract numeric part (handles "1", "1 day", "1.5", etc.)
            var numericPart = "";
            bool foundDecimal = false;
            foreach (char c in value)
            {
                if (char.IsDigit(c)) numericPart += c;
                else if (c == '.' && !foundDecimal) { numericPart += c; foundDecimal = true; }
                else if (numericPart.Length > 0) break; // Stop after first numeric group
            }

            if (double.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double days))
            {
                _isUpdatingDuration = true;
                PlannedDuration = $"{days} {(days == 1 ? "day" : "days")}";
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

        [RelayCommand]
        public void CommitDurations()
        {
            FormatPlannedDuration(PlannedDuration);
            FormatActualDuration(ActualDuration);
        }

        async partial void OnPlannedHoursChanged(double? value) => await UpdateTask();
        async partial void OnActualHoursChanged(double? value) => await UpdateTask();
        async partial void OnDescriptionChanged(string value) => await UpdateTask();
        async partial void OnTitleChanged(string value) => await UpdateTask();
        async partial void OnIsCompletedChanged(bool? value) 
        {
             // Special handling for Done Checkbox (could set DoneDate automatically)
             if(value == true && DoneDate == null) DoneDate = DateTime.Now;
             if(value == false) DoneDate = null;
             
             await UpdateTask();
        }

        private async System.Threading.Tasks.Task UpdateTask()
        {
            if (_currentTaskId == Guid.Empty) return;

            var task = await _projectTaskRepository.GetByIdAsync(_currentTaskId);
            if (task == null) return;

            // Update fields from properties
            task.Name = Title;
            task.Description = Description;
            
            task.PlanedStartDate = PlannedStartDate;
            task.PlanedDueDate = DueDate;
            task.ActualStartDate = ActualStartDate;
            task.ActualCompleteDate = DoneDate;

            if (PlannedHours.HasValue)
                task.PlanedDurationHours = TimeSpan.FromHours(PlannedHours.Value);
            else
                task.PlanedDurationHours = null;

            if (ActualHours.HasValue)
                task.ActualDuration = TimeSpan.FromHours(ActualHours.Value);
            else
                task.ActualDuration = null;

            await _projectTaskRepository.UpdateAsync(task);
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
                Comments.Insert(0, new TaskComment
                {
                    AuthorName = "Current User",
                    AuthorEmail = "user@occ.com",
                    Content = NewCommentContent,
                    CreatedAt = DateTime.Now
                });
                NewCommentContent = string.Empty;
                OnPropertyChanged(nameof(CommentsCount));
            }
        }

        public event EventHandler? CloseRequested;

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
