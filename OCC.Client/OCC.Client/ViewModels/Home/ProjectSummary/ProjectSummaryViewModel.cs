using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using OCC.Client.Services;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Home.ProjectSummary
{
    public partial class ProjectSummaryViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;

        #endregion

        #region Observables

        [ObservableProperty]
        private double _pipelinePlanning = 1;

        [ObservableProperty]
        private double _totalBudget = 0;

        [ObservableProperty]
        private double _totalCost = 0;

        [ObservableProperty]
        private int _notStartedCount;

        [ObservableProperty]
        private int _inProgressCount;
        
        [ObservableProperty]
        private int _completedCount;

        [ObservableProperty]
        private int _totalTaskCount;

        // Chart Segments (Angles for arcs)
        [ObservableProperty]
        private double _notStartedAngle;
        
        [ObservableProperty]
        private double _inProgressAngle;
        
        [ObservableProperty]
        private double _completedAngle;

        [ObservableProperty]
        private double _notStartedStartAngle;

        [ObservableProperty]
        private double _inProgressStartAngle;

        [ObservableProperty]
        private double _completedStartAngle;
        
        #endregion

        #region Properties

        public ObservableCollection<PortfolioProjectItem> Projects { get; } = new();
        public ObservableCollection<TeamPulseItem> TeamPulse { get; } = new();

        #endregion

        #region Constructors

        public ProjectSummaryViewModel(IRepository<ProjectTask> taskRepository)
        {
            _taskRepository = taskRepository;

            // Seed Mock Data
            Projects.Add(new PortfolioProjectItem
            {
                Name = "Construction Schedule",
                TimeStatus = "Green",
                CostStatus = "Gray",
                WorkloadStatus = "Green",
                Progress = 7,
                TotalCost = 0
            });

            TeamPulse.Add(new TeamPulseItem { Name = "Jennifer Jones (Sample)", Initials = "JJ", Color = "#F59E0B", Count = 4 });
            TeamPulse.Add(new TeamPulseItem { Name = "Mike Smith (Sample)", Initials = "MS", Color = "#D9F99D", Count = 4 }); 
            TeamPulse.Add(new TeamPulseItem { Name = "origize63@gmail.com", Initials = "OR", Color = "#2DD4BF", Count = 0 }); 
            TeamPulse.Add(new TeamPulseItem { Name = "Sam Watson (Sample)", Initials = "SW", Color = "#A3E635", Count = 3 });

            LoadTaskStatistics();
        }

        // Temporary zero-argument constructor for XAML preview/design time
        public ProjectSummaryViewModel() : this(new MockProjectTaskRepository()) { }

        #endregion

        #region Methods

        private async void LoadTaskStatistics()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var allTasks = tasks.ToList();

            TotalTaskCount = allTasks.Count;

            // Define "In Progress" as started but not completed
            var now = DateTime.Now.Date;
            
            // Logic: 
            // Completed: ActualCompleteDate has value OR Status == "Completed"
            // In Progress: No CompleteDate, but (ActualStartDate has value OR StartDate <= Today)
            // Not Started: Everything else (StartDate > Today and no ActualStartDate)
            
            CompletedCount = allTasks.Count(t => t.ActualCompleteDate.HasValue || t.Status == "Completed"); // Added Status check as ProjectTask uses Status
            
            InProgressCount = allTasks.Count(t => 
                !t.ActualCompleteDate.HasValue && t.Status != "Completed" &&
                (t.ActualStartDate.HasValue || t.StartDate.Date <= now));

            NotStartedCount = allTasks.Count(t => 
                !t.ActualCompleteDate.HasValue && t.Status != "Completed" &&
                !t.ActualStartDate.HasValue && 
                t.StartDate.Date > now);

            CalculateChartAngles();
        }

        #endregion

        #region Helper Methods

        private void CalculateChartAngles()
        {
            if (TotalTaskCount == 0) return;

            // Calculate sweep angles (proportions of 360)
            double notStartedSweep = (double)NotStartedCount / TotalTaskCount * 360;
            double inProgressSweep = (double)InProgressCount / TotalTaskCount * 360;
            double completedSweep = (double)CompletedCount / TotalTaskCount * 360;

            NotStartedAngle = notStartedSweep;
            InProgressAngle = inProgressSweep;
            CompletedAngle = completedSweep;

            // Calculate Start Angles (accumulated)
            // Order: Not Started (Gray) -> In Progress (Blue) -> Completed (Green)
            // Starting from -90 (Top)
            
            NotStartedStartAngle = -90;
            InProgressStartAngle = NotStartedStartAngle + NotStartedAngle;
            CompletedStartAngle = InProgressStartAngle + InProgressAngle;
        }

        #endregion
    }

    public class PortfolioProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string TimeStatus { get; set; } = "Green"; // Green, Red, Gray
        public string CostStatus { get; set; } = "Gray";
        public string WorkloadStatus { get; set; } = "Green";
        public int Progress { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class TeamPulseItem
    {
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Color { get; set; } = "#CCCCCC";
        public int Count { get; set; }
    }
}
