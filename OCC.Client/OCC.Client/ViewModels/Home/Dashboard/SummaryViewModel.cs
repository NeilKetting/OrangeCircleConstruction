using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OCC.Client.ViewModels.Home.Dashboard
{
    public partial class SummaryViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly ILogger<SummaryViewModel> _logger;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _totalProjects = "0";

        [ObservableProperty]
        private string _activeProjects = "0";

        [ObservableProperty]
        private string _projectsCompleted = "0";

        [ObservableProperty]
        private int _notStartedCount;

        [ObservableProperty]
        private int _inProgressCount;
        
        [ObservableProperty]
        private int _completedCount;

        [ObservableProperty]
        private int _totalTaskCount;

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

        [ObservableProperty]
        private double _totalActualHours;

        [ObservableProperty]
        private double _totalPlannedHours;

        [ObservableProperty]
        private double _timeChartAngle;

        [ObservableProperty]
        private double _timeChartStartAngle = -90;

        [ObservableProperty]
        private string _timeChartColor = "#22C55E"; // Green by default

        [ObservableProperty]
        private double _timeEfficiencyPercentage;

        #endregion

        #region Constructors

        public SummaryViewModel(IRepository<Project> projectRepository, IRepository<ProjectTask> taskRepository, ILogger<SummaryViewModel> logger)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _logger = logger;
            LoadData();
        }

        // Design-time constructor removed as mocks are deleted
        // public SummaryViewModel() : this(new MockProjectRepository(), new MockProjectTaskRepository()) { }

        #endregion

        #region Methods

        private async void LoadData()
        {
            try
            {
                // Load Projects
                var projects = await _projectRepository.GetAllAsync();
                var projectList = projects.ToList();

                TotalProjects = projectList.Count.ToString();
                ActiveProjects = projectList.Count(p => p.Status == "Active").ToString();
                ProjectsCompleted = projectList.Count(p => p.Status == "Completed").ToString();

                // Load Tasks
                // Use ApiProjectTaskRepository to get tasks assigned to the current user
                IEnumerable<ProjectTask> tasks;
                if (_taskRepository is ApiProjectTaskRepository apiRepo)
                {
                    tasks = await apiRepo.GetMyTasksAsync();
                }
                else
                {
                    // Fallback for design time or mock
                    tasks = await _taskRepository.GetAllAsync();
                }

                var allTasks = tasks.ToList();
                TotalTaskCount = allTasks.Count;

                var now = System.DateTime.Now.Date;

                CompletedCount = allTasks.Count(t => t.ActualCompleteDate.HasValue);
                
                InProgressCount = allTasks.Count(t => 
                    !t.ActualCompleteDate.HasValue && 
                    (t.ActualStartDate.HasValue || (t.StartDate.Date <= now)));

                NotStartedCount = allTasks.Count(t => 
                    !t.ActualCompleteDate.HasValue && 
                    !t.ActualStartDate.HasValue && 
                    (t.StartDate.Date > now));

                CalculateTimeStatistics(allTasks);
                CalculateChartAngles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading summary data");
            }
        }

        #endregion

        #region Helper Methods

        private void CalculateChartAngles()
        {
            if (TotalTaskCount == 0) return;

            double notStartedSweep = (double)NotStartedCount / TotalTaskCount * 360;
            double inProgressSweep = (double)InProgressCount / TotalTaskCount * 360;
            double completedSweep = (double)CompletedCount / TotalTaskCount * 360;

            NotStartedAngle = notStartedSweep;
            InProgressAngle = inProgressSweep;
            CompletedAngle = completedSweep;

            NotStartedStartAngle = -90;
            InProgressStartAngle = NotStartedStartAngle + NotStartedAngle;
            CompletedStartAngle = InProgressStartAngle + InProgressAngle;
        }

        private void CalculateTimeStatistics(System.Collections.Generic.List<ProjectTask> allTasks)
        {
            double planned = 0;
            double actual = 0;

            foreach (var t in allTasks)
            {
                if (t.PlanedDurationHours.HasValue)
                    planned += t.PlanedDurationHours.Value.TotalHours;

                if (t.ActualDuration.HasValue)
                    actual += t.ActualDuration.Value.TotalHours;
            }

            TotalPlannedHours = Math.Round(planned, 1);
            TotalActualHours = Math.Round(actual, 1);

            if (TotalPlannedHours > 0)
            {
                // Efficiency Ratio: Actual / Planned
                double ratio = actual / planned;
                
                // If Ratio > 1, we are over budget (Red)
                // If Ratio <= 1, we are under/on budget (Green)
                if (ratio > 1.0)
                {
                    TimeChartColor = "#EF4444"; // Red 500
                    TimeChartAngle = 360; 
                    TimeEfficiencyPercentage = 100;
                }
                else
                {
                    TimeChartColor = "#22C55E"; // Green 500
                    TimeChartAngle = ratio * 360;
                    TimeEfficiencyPercentage = ratio * 100;
                }
            }
            else
            {
                TimeChartAngle = 0;
                TimeEfficiencyPercentage = 0;
            }
        }

        #endregion
    }
}
