using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Home.Calendar
{
    public partial class CalendarViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;

        #endregion

        #region Observables

        [ObservableProperty]
        private DateTime _currentMonth;

        [ObservableProperty]
        private string _monthName = string.Empty;

        [ObservableProperty]
        private string _yearName = string.Empty; 

        [ObservableProperty]
        private bool _isCreatePopupVisible;

        [ObservableProperty]
        private CreateTaskPopupViewModel? _createTaskPopup;

        [ObservableProperty]
        private DateTime _currentDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<CalendarDayViewModel> _days = new();

        [ObservableProperty]
        private ObservableCollection<string> _weekDays = new() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        [ObservableProperty] 
        private bool _isBusy;

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _dayTasks = new();

        #endregion

        #region Constructors

        public CalendarViewModel()
        {
            _taskRepository = new MockProjectTaskRepository();
            _projectRepository = new MockProjectRepository();
            _authService = new MockAuthService(new MockUserRepository()); 
            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            GenerateCalendar();
        }
        
        public CalendarViewModel(IRepository<ProjectTask> taskRepository, IRepository<Project> projectRepository, IAuthService authService)
        {
             _taskRepository = taskRepository;
             _projectRepository = projectRepository;
             _authService = authService;
             CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
             GenerateCalendar();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            GenerateCalendar();
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            GenerateCalendar();
        }

        [RelayCommand]
        public void OpenCreatePopup(DateTime date)
        {
            CreateTaskPopup = new CreateTaskPopupViewModel(_taskRepository, _projectRepository, _authService);
            CreateTaskPopup.SetDate(date);
            CreateTaskPopup.CloseRequested += (s, e) => CloseCreatePopup();
            CreateTaskPopup.TaskCreated += async (s, e) => await LoadTasks();
            IsCreatePopupVisible = true;
        }

        [RelayCommand]
        public void CloseCreatePopup()
        {
             IsCreatePopupVisible = false;
             CreateTaskPopup = null;
        }

        #endregion

        #region Methods

        private async void GenerateCalendar()
        {
            MonthName = CurrentMonth.ToString("MMMM");
            YearName = CurrentMonth.ToString("yyyy");
            Days.Clear();

            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            
            int offset = (int)firstDayOfMonth.DayOfWeek - 1; 
            if (offset < 0) offset = 6; 

            var previousMonth = CurrentMonth.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            for (int i = 0; i < offset; i++)
            {
                var day = daysInPrevMonth - offset + 1 + i;
                var date = new DateTime(previousMonth.Year, previousMonth.Month, day);
                Days.Add(new CalendarDayViewModel(date, false));
            }

            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, i);
                Days.Add(new CalendarDayViewModel(date, true));
            }

            int remaining = 42 - Days.Count;
            var nextMonth = CurrentMonth.AddMonths(1);
            for (int i = 1; i <= remaining; i++)
            {
                var date = new DateTime(nextMonth.Year, nextMonth.Month, i);
                Days.Add(new CalendarDayViewModel(date, false));
            }

            await LoadTasks();
        }

        private async System.Threading.Tasks.Task LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            var sortedTasks = tasks
                .OrderBy(t => GetTaskStart(t))
                .ThenByDescending(t => (GetTaskEnd(t) - GetTaskStart(t)).Days)
                .ToList();

            foreach (var day in Days) day.Tasks.Clear();

            // Process per week (chunks of 7 days)
            for (int i = 0; i < Days.Count; i += 7)
            {
                var weekDays = Days.Skip(i).Take(7).ToList();
                var weekStart = weekDays.First().Date;
                var weekEnd = weekDays.Last().Date;

                // Find tasks visible in this week
                var weekTasks = sortedTasks.Where(t => 
                    GetTaskStart(t) <= weekEnd && GetTaskEnd(t) >= weekStart).ToList();

                // Slot tracking for this week: index -> date which it is busy until
                var slots = new System.Collections.Generic.List<DateTime>();

                foreach (var task in weekTasks)
                {
                    DateTime taskStart = GetTaskStart(task);
                    DateTime taskEnd = GetTaskEnd(task);
                    
                    // Clamp to week
                    DateTime effectiveStart = taskStart < weekStart ? weekStart : taskStart;
                    DateTime effectiveEnd = taskEnd > weekEnd ? weekEnd : taskEnd;

                    // Find a slot
                    int slotIndex = -1;
                    for (int s = 0; s < slots.Count; s++)
                    {
                        if (slots[s] < effectiveStart.Date)
                        {
                            slotIndex = s;
                            slots[s] = effectiveEnd.Date;
                            break;
                        }
                    }
                    if (slotIndex == -1)
                    {
                        slotIndex = slots.Count;
                        slots.Add(effectiveEnd.Date);
                    }

                    // Populate days
                    foreach (var day in weekDays)
                    {
                        if (day.Date >= effectiveStart.Date && day.Date <= effectiveEnd.Date)
                        {
                            var vm = new CalendarTaskViewModel
                            {
                                Id = task.Id,
                                Name = task.Name,
                                VisualSlotIndex = slotIndex,
                                Start = taskStart,
                                End = taskEnd,
                                IsCompleted = task.ActualCompleteDate.HasValue,
                                Color = GetTaskColor(task.Id)
                            };

                            bool isTrueStart = day.Date == taskStart;
                            bool isTrueEnd = day.Date == taskEnd;
                            
                            if (isTrueStart && isTrueEnd) vm.SpanType = CalendarTaskSpanType.Single;
                            else if (isTrueStart) vm.SpanType = CalendarTaskSpanType.Start;
                            else if (isTrueEnd) vm.SpanType = CalendarTaskSpanType.End;
                            else vm.SpanType = CalendarTaskSpanType.Middle;

                            day.Tasks.Add(vm);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private string GetTaskColor(Guid taskId)
        {
            string[] palette = 
            { 
                "#3B82F6", // Blue
                "#10B981", // Emerald
                "#8B5CF6", // Violet
                "#F59E0B", // Amber
                "#F43F5E", // Rose
                "#06B6D4", // Cyan
                "#6366F1", // Indigo
                "#14B8A6"  // Teal
            };
            
            // Use hash of Guid to pick a consistent color
            int index = Math.Abs(taskId.GetHashCode()) % palette.Length;
            return palette[index];
        }

        private DateTime GetTaskStart(ProjectTask t) => t.StartDate.Date;
        private DateTime GetTaskEnd(ProjectTask t) => t.FinishDate.Date;

        #endregion
    }
}
