using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Time;
using OCC.Client.ViewModels.Team;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Notifications;
using OCC.Client.ViewModels.Shared;

namespace OCC.Client.ViewModels.Home
{
    public partial class HomeViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly ITimeService _timeService;
        private readonly IRepository<TaskItem> _projectTaskRepository;

        [ObservableProperty]
        private SidebarViewModel _sidebar;

        [ObservableProperty]
        private TopBarViewModel _topBar;

        [ObservableProperty]
        private SummaryViewModel _mySummary;

        [ObservableProperty]
        private TasksWidgetViewModel _myTasks;

        [ObservableProperty]
        private PulseViewModel _projectPulse;

        [ObservableProperty]
        private ProjectSummaryViewModel _projectSummary;

        [ObservableProperty]
        private TeamSummaryViewModel _teamSummary;

        [ObservableProperty]
        private bool _isMySummaryVisible = true;

        [ObservableProperty]
        private bool _isTeamSummaryVisible = false;

        [ObservableProperty]
        private bool _isProjectSummaryVisible = false;

        [ObservableProperty]
        private TaskListViewModel _listViewModel;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

        [ObservableProperty]
        private Calendar.CalendarViewModel _calendar;

        [ObservableProperty]
        private Time.TimeViewModel _time;

        [ObservableProperty]
        private TeamViewModel _team;

        [ObservableProperty]
        private ProjectsViewModel _projects;

        [ObservableProperty]
        private NotificationsViewModel _notifications;

        [ObservableProperty]
        private bool _isDashboardVisible = true;

        [ObservableProperty]
        private bool _isListVisible = false;

        [ObservableProperty]
        private bool _isCalendarVisible = false;

        [ObservableProperty]
        private bool _isTimeVisible = false;

        [ObservableProperty]
        private bool _isTeamVisible = false;

        [ObservableProperty]
        private bool _isProjectsVisible = false;

        [ObservableProperty]
        private bool _isNotificationsVisible = false;

        [ObservableProperty]
        private bool _isTaskDetailVisible = false;

        [ObservableProperty]
        private TaskDetailViewModel? _currentTaskDetail;

        [ObservableProperty]
        private bool _isRollCallVisible = false;

        [ObservableProperty]
        private RollCallViewModel? _currentRollCall;

        public bool IsTopBarVisible => Sidebar.ActiveSection == "Home";

        public HomeViewModel(SidebarViewModel sidebar, 
                             TopBarViewModel topBar, 
                             SummaryViewModel mySummary, 
                             TasksWidgetViewModel myTasks, 
                             PulseViewModel projectPulse,
                             ProjectSummaryViewModel projectSummary,
                             TaskListViewModel listViewModel,
                             IAuthService authService,
                             ITimeService timeService,
                             IRepository<TaskItem> projectTaskRepository,
                             IRepository<Project> projectRepository)
        {
            _authService = authService;
            _timeService = timeService;
            _projectTaskRepository = projectTaskRepository;
            Sidebar = sidebar;
            TopBar = topBar;
            MySummary = mySummary;
            MyTasks = myTasks;
            ProjectPulse = projectPulse;
            ProjectSummary = projectSummary;
            ListViewModel = listViewModel;
            
            // Initialize Calendar and Time
            Calendar = new Calendar.CalendarViewModel(_projectTaskRepository, projectRepository, _authService);
            Time = new TimeViewModel(timeService, _authService);
            Team = new TeamViewModel();
            Projects = new ProjectsViewModel();
            TeamSummary = new TeamSummaryViewModel();
            Notifications = new NotificationsViewModel();
            
            // Subscribe to list selection
            ListViewModel.TaskSelectionRequested += (s, e) => OpenTaskDetail(Guid.Parse(e));

            Sidebar.PropertyChanged += Sidebar_PropertyChanged;
            TopBar.PropertyChanged += TopBar_PropertyChanged;

            Initialize();
        }

        private async void Initialize()
        {
            var now = DateTime.Now;
            Greeting = GetGreeting(now);
            CurrentDate = now.ToString("dd MMMM yyyy");
        }

        private void Sidebar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.ActiveSection))
            {
                OnPropertyChanged(nameof(IsTopBarVisible));
            }
        }

        private void TopBar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TopBarViewModel.ActiveTab))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            IsDashboardVisible = false;
            IsListVisible = false;
            IsCalendarVisible = false;
            IsTimeVisible = false;
            IsTeamVisible = false;
            IsProjectsVisible = false;
            IsNotificationsVisible = false;
            IsMySummaryVisible = false;
            IsTeamSummaryVisible = false;
            IsProjectSummaryVisible = false;

            switch (TopBar.ActiveTab)
            {
                case "List":
                    IsListVisible = true;
                    break;
                case "Calendar":
                    IsCalendarVisible = true;
                    break;
                case "Time":
                    IsTimeVisible = true;
                    break;
                case "Team":
                    IsTeamVisible = true;
                    break;
                case "Projects":
                    IsProjectsVisible = true;
                    break;
                case "Notifications":
                    IsNotificationsVisible = true;
                    break;
                case "RollCall":
                    OpenRollCall();
                    break;
                case "Portfolio Summary": // Keeping the key string as "Portfolio Summary" to match TopBar for now, unless we change TopBar too. 
                // User asked to name it "project" from the mockup name "portfolio". The mockup says "Portfolio Summary". 
                // Wait, "The mockup names it portfolio but we will name it project".
                // So I should rename the Tab in TopBar too.
                // Assuming I will change TopBar key to "Project Summary".
                case "Project Summary":
                    IsDashboardVisible = true;
                    IsProjectSummaryVisible = true;
                    break;
                case "Team Summary":
                    IsDashboardVisible = true;
                    IsTeamSummaryVisible = true;
                    break;
                case "My Summary":
                default:
                    if (TopBar.ActiveTab == "My Summary" || TopBar.ActiveTab == "Team Summary")
                    {
                         IsDashboardVisible = true;
                         IsMySummaryVisible = true;
                    }
                    else 
                    {
                        IsDashboardVisible = true;
                        IsMySummaryVisible = true;
                    }
                    break;
            }
        }

        [RelayCommand]
        private void OpenTaskDetail(Guid taskId)
        {
            CurrentTaskDetail = new TaskDetailViewModel(_projectTaskRepository);
            CurrentTaskDetail.CloseRequested += (s, e) => CloseTaskDetail();
            CurrentTaskDetail.LoadTaskById(taskId);
            IsTaskDetailVisible = true;
        }

        [RelayCommand]
        private void CloseTaskDetail()
        {
            IsTaskDetailVisible = false;
            CurrentTaskDetail = null;
        }

        private string GetGreeting(DateTime time)
        {
            string timeGreeting = time.Hour < 12 ? "Good morning" :
                                  time.Hour < 18 ? "Good afternoon" : "Good evening";
            
            var userName = _authService.CurrentUser?.DisplayName ?? "User";
            return $"{timeGreeting}, {userName}";
        }

        [RelayCommand]
        private void OpenRollCall()
        {
            CurrentRollCall = new RollCallViewModel(_timeService);
            CurrentRollCall.CloseRequested += (s, e) => CloseRollCall();
            IsRollCallVisible = true;
        }

        [RelayCommand]
        private void CloseRollCall()
        {
            IsRollCallVisible = false;
            CurrentRollCall = null;
        }
    }
}
