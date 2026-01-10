using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using OCC.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectGanttViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IDialogService _dialogService;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<GanttTaskWrapper> _ganttTasks = new();
        
        [ObservableProperty]
        private double _zoomLevel = 1.0;

        [ObservableProperty]
        private DateTime _projectStartDate = DateTime.Now;

        [ObservableProperty]
        private double _pixelsPerDay = 50.0; // Adjustable zoom

        [ObservableProperty]
        private double _rowHeight = 24.0;

        [ObservableProperty]
        private double _canvasWidth = 3000;

        [ObservableProperty]
        private double _canvasHeight = 600;

        #endregion

        #region Properties

        public ObservableCollection<GanttDateHeader> DateHeaders { get; } = new();
        public ObservableCollection<GanttDependencyLine> Dependencies { get; } = new();

        #endregion

        #region Constructors

        public ProjectGanttViewModel()
        {
            // Parameterless constructor for design-time support
            _taskRepository = null!;
            _dialogService = null!;
        }

        public ProjectGanttViewModel(IRepository<ProjectTask> taskRepository, IDialogService dialogService)
        {
            _taskRepository = taskRepository;
            _dialogService = dialogService;
        }

        #endregion

        #region Methods

        public async void LoadTasks(Guid projectId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectGanttViewModel] Loading Tasks for Project {projectId}...");
                var tasks = await _taskRepository.FindAsync(t => t.ProjectId == projectId);
                GanttTasks.Clear();
                DateHeaders.Clear();
                Dependencies.Clear();
                
                DateTime minDate = DateTime.MaxValue;
                DateTime maxDate = DateTime.MinValue;

                var taskList = new List<ProjectTask>(tasks);
                taskList = taskList.OrderBy(t => t.OrderIndex).ToList();

                foreach (var task in taskList)
                {
                     // Ignore unscheduled tasks for range calculation
                     if (task.StartDate > DateTime.MinValue && task.StartDate < minDate) minDate = task.StartDate;
                     if (task.FinishDate > DateTime.MinValue && task.FinishDate > maxDate) maxDate = task.FinishDate;
                }

                if (minDate != DateTime.MaxValue)
                    ProjectStartDate = minDate.AddDays(-7); 
                else
                    ProjectStartDate = DateTime.Now.AddDays(-14);

                // If maxDate is still MinValue (no tasks scheduled), set a default lookahead
                if (maxDate == DateTime.MinValue) maxDate = ProjectStartDate.AddDays(30);

                GenerateHeaders(ProjectStartDate, maxDate.AddDays(30));

                // Calculate Canvas Width
                var days = (maxDate.AddDays(30) - ProjectStartDate).TotalDays;
                CanvasWidth = Math.Max(3000, days * PixelsPerDay);


                int index = 0;
                double headerOffset = 4.0; // Centering 16px bar in 24px row
                
                // First pass: Create Wrappers
                var idToWrapperMap = new Dictionary<string, GanttTaskWrapper>();
                
                foreach (var task in taskList)
                {
                    if (task.StartDate == DateTime.MinValue) continue; // Skip unscheduled tasks

                    var wrapper = new GanttTaskWrapper(task, ProjectStartDate, PixelsPerDay, index, headerOffset, RowHeight);
                    GanttTasks.Add(wrapper);
                    idToWrapperMap[task.Id.ToString()] = wrapper;
                    index++;
                }
                
                CanvasHeight = Math.Max(600, index * RowHeight + 100);

                // Second pass: Generate Dependencies
                GenerateDependencies(idToWrapperMap);
                
                // Third pass: Force Visual Containment (View-Side Fix)
                // This ensures that if the list shows indentation, the parent BAR physically contains the children
                // regardless of database values or XML anomalies.
                HarmonizeVisualDates(GanttTasks.ToList());
                System.Diagnostics.Debug.WriteLine($"[ProjectGanttViewModel] LoadTasks Complete. {GanttTasks.Count} tasks rendered.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectGanttViewModel] CRASH in LoadTasks: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ProjectGanttViewModel] Stack: {ex.StackTrace}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading Gantt chart: {ex.Message}");
                }
            }
        }


        #endregion

        #region Helper Methods

        private void HarmonizeVisualDates(List<GanttTaskWrapper> wrappers)
        {
            // 1. Build Visual Hierarchy
            var parentStack = new Stack<GanttTaskWrapper>();
            
            foreach (var wrapper in wrappers)
            {
                // Pop until we find the parent (IndentLevel must be less than current)
                while (parentStack.Count > 0 && parentStack.Peek().Task.IndentLevel >= wrapper.Task.IndentLevel)
                {
                    parentStack.Pop();
                }
                
                if (parentStack.Count > 0)
                {
                    var parent = parentStack.Peek();
                    parent.ChildrenWrappers.Add(wrapper);
                }
                
                parentStack.Push(wrapper);
            }
            
            // 2. Bubble Up Dates (Iterate backwards or recursive)
            // Since we populated ChildrenWrappers, we can just recurse from Roots or iterate backwards.
            // Reverse iteration is safe for bubbling up.
            
            for (int i = wrappers.Count - 1; i >= 0; i--)
            {
                var wrapper = wrappers[i];
                if (wrapper.ChildrenWrappers.Count > 0)
                {
                    // It is a group/summary visually
                    // Determine Extents of Children
                    double minLeft = double.MaxValue;
                    double maxRight = double.MinValue;
                    bool hasChildren = false;

                    foreach (var child in wrapper.ChildrenWrappers)
                    {
                        if (child.Left < minLeft) minLeft = child.Left;
                        if (child.Right > maxRight) maxRight = child.Right;
                        hasChildren = true;
                    }
                    
                    if (hasChildren && minLeft != double.MaxValue && maxRight != double.MinValue)
                    {
                        // Apply Containment Logic
                         wrapper.Left = minLeft;
                         wrapper.Width = maxRight - minLeft;
                         
                         // Fix visual quirk: if width is too small?
                         if (wrapper.Width < 10) wrapper.Width = 20; // Ensure visibility
                         
                         // Important: Setting Left/Width here overrides the initial calculation from Task.Dates.
                         // This solves the 'Math Problem' by syncing the Bar to the Visual Children.
                         
                         // Also force IsSummary styling if it has children visually
                         // (Wrapper.IsSummary is readonly, but usually matches IsGroup).
                    }
                }
            }
        }

        private void GenerateDependencies(Dictionary<string, GanttTaskWrapper> map)
        {
            foreach (var wrapper in GanttTasks)
            {
                foreach (var predString in wrapper.Task.Predecessors)
                {
                    var parts = predString.Split('|');
                    var predId = parts[0];
                    int type = 1; // Default FS
                    if (parts.Length > 1 && int.TryParse(parts[1], out var t)) type = t;

                    if (map.TryGetValue(predId, out var predWrapper))
                    {
                        Dependencies.Add(new GanttDependencyLine(predWrapper, wrapper, type));
                    }
                }
            }
        }
        
        private void GenerateHeaders(DateTime start, DateTime end)
        {
            DateHeaders.Clear();
            var current = start;
            int index = 0;
            while (current <= end)
            {
                double left = (current - ProjectStartDate).TotalDays * PixelsPerDay;
                // Add header
                DateHeaders.Add(new GanttDateHeader 
                { 
                    Text = current.ToString("dd MMM"),
                    Left = left + 5, // Small offset for text
                    // Store the raw Left for the column rectangle
                    ColumnLeft = left,
                    Width = PixelsPerDay,
                    IsAlternate = (index % 2 == 1) // Alternating columns
                });
                current = current.AddDays(1);
                index++;
            }
        }

        #endregion
    }

    public class GanttDependencyLine
    {
        public Avalonia.Media.StreamGeometry PathGeometry { get; private set; }
        public Avalonia.Media.StreamGeometry ArrowGeometry { get; private set; }

        public GanttDependencyLine(GanttTaskWrapper predecessor, GanttTaskWrapper successor, int type)
        {
            // Standard FS: Predecessor Right -> Successor Left
            var start = new Avalonia.Point(predecessor.Left + predecessor.Width, predecessor.Top + (predecessor.Height / 2));
            var end = new Avalonia.Point(successor.Left, successor.Top + (successor.Height / 2));

            var geometry = new Avalonia.Media.StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false);

                if (end.X > start.X + 20)
                {
                    // Normal Case: Successor is well to the right
                    // Path: Start -> Right to Mid -> Vertical -> End
                    double midX = start.X + (end.X - start.X) / 2;
                    context.LineTo(new Avalonia.Point(midX, start.Y));
                    context.LineTo(new Avalonia.Point(midX, end.Y));
                    context.LineTo(end);
                }
                else
                {
                    // Overlap Case: Successor starts before Predecessor ends
                    // Path: Start -> Right -> Down -> Left -> Down -> Right
                    double midY = (start.Y + end.Y) / 2;
                    if (Math.Abs(start.Y - end.Y) < 10) midY = start.Y + 15; // avoiding overlapping line if rows adjacent

                    // Push out right
                    context.LineTo(new Avalonia.Point(start.X + 10, start.Y));
                    // Vertical to MidY
                    context.LineTo(new Avalonia.Point(start.X + 10, midY));
                    // Horizontal Back
                    context.LineTo(new Avalonia.Point(end.X - 10, midY));
                    // Vertical to EndY
                    context.LineTo(new Avalonia.Point(end.X - 10, end.Y));
                    // In to End
                    context.LineTo(end);
                }
            }
            PathGeometry = geometry;

            // Arrow at End
            var arrow = new Avalonia.Media.StreamGeometry();
            using (var ctx = arrow.Open())
            {
                // Arrowhead pointing Right at End point
                ctx.BeginFigure(end, true);
                ctx.LineTo(new Avalonia.Point(end.X - 6, end.Y - 3));
                ctx.LineTo(new Avalonia.Point(end.X - 6, end.Y + 3));
                ctx.EndFigure(true);
            }
            ArrowGeometry = arrow;
        }
    }

    public class GanttDateHeader
    {
        public string Text { get; set; } = string.Empty;
        public double Left { get; set; }
        public double ColumnLeft { get; set; }
        public double Width { get; set; }
        public bool IsAlternate { get; set; }
    }

    public class GanttTaskWrapper : ObservableObject
    {
        public ProjectTask Task { get; }
        
        private double _left;
        public double Left 
        { 
            get => _left; 
            set => SetProperty(ref _left, value); 
        }

        private double _width;
        public double Width 
        { 
            get => _width; 
            set {
                if (SetProperty(ref _width, value))
                {
                    OnPropertyChanged(nameof(Right));
                }
            }
        }
        
        public double Right => Left + Width;

        public double Top { get; }
        public double Height { get; } = 20;
        public bool IsSummary { get; }
        public bool IsAlternate { get; } // For zebra striping
        public string LabelText { get; } // Display Name
        
        public CommunityToolkit.Mvvm.Input.RelayCommand? ToggleCommand { get; } 
        public Avalonia.Thickness IndentMargin { get; }
        
        public List<GanttTaskWrapper> ChildrenWrappers { get; } = new();

        public GanttTaskWrapper(ProjectTask task, DateTime projectStart, double pixelsPerDay, int index, double topOffset, double rowHeight)
        {
            Task = task;
            IsSummary = task.IsGroup;
            IsAlternate = index % 2 != 0;
            IndentMargin = new Avalonia.Thickness(task.IndentLevel * 15, 0, 0, 0);
            
            // Format Label: "Check plumbing 0% NB, JD"
            string resources = string.IsNullOrEmpty(task.AssignedTo) ? "" : task.AssignedTo;
            LabelText = $"{task.Name}  {task.PercentComplete}%  {resources}";
            
            var startOffset = (task.StartDate - projectStart).TotalDays;
            if (startOffset < 0) startOffset = 0;
            
            _left = startOffset * pixelsPerDay;
            
            var durationDays = (task.FinishDate - task.StartDate).TotalDays;
            if (durationDays < 0.5) durationDays = 1.0; 
            
            _width = durationDays * pixelsPerDay;
            
            // Row Height Logic
            RowHeight = rowHeight;
            RowTop = index * rowHeight;
            Top = RowTop + topOffset; 
        }

        public double RowHeight { get; }
        public double RowTop { get; }
    }
}
