using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.ViewModels;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskTreeItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTime _dueDate;

        [ObservableProperty]
        private string _status = string.Empty;

        [ObservableProperty]
        private string _priority = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private string _assigneeInitials = string.Empty;

        [ObservableProperty]
        private int _commentsCount;

        [ObservableProperty]
        private int _attachmentsCount;

        [ObservableProperty]
        private bool _isExpanded = false;

        [ObservableProperty]
        private bool _isGroup; // If true, it might be a summary task

        public ObservableCollection<TaskTreeItemViewModel> Children { get; } = new();

        public TaskTreeItemViewModel(ProjectTask task)
        {
            Id = task.Id;
            Title = task.Name;
            Description = task.Description;
            DueDate = task.FinishDate;
            Status = task.Status;
            Priority = task.Priority;
            IsCompleted = task.IsComplete;
            AssigneeInitials = !string.IsNullOrEmpty(task.AssignedTo) && task.AssignedTo.Length >= 2 
                ? task.AssignedTo.Substring(0, 2).ToUpper() : "??";
            
            CommentsCount = task.Comments?.Count ?? 0;
            AttachmentsCount = 0; // Placeholder
            IsGroup = task.IsGroup || task.Children?.Count > 0; // Fallback logic
            IndentLevel = task.IndentLevel;
        }

        [ObservableProperty]
        private int _indentLevel;

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
