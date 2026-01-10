using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Home.MySummary;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Projects;
using OCC.Shared.Models;
using System;
using Microsoft.Extensions.Logging;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Home
{
    public partial class HomeViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IAuthService _authService;
        private readonly ITimeService _timeService;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ProjectTask> _projectTaskModelRepository;
        private readonly IRepository<AppSetting> _appSettingsRepository;
        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IRepository<TaskComment> _commentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IDialogService _dialogService;
        private readonly ILoggerFactory _loggerFactory;

        #endregion

        #region Observables

        [ObservableProperty]
        private HomeMenuViewModel _homeMenu;

        [ObservableProperty]
        private ViewModelBase _currentView;

        [ObservableProperty]
        private MySummaryPageViewModel _mySummaryPage;

        [ObservableProperty]
        private ProjectSummaryPageViewModel _projectSummaryPage;

        [ObservableProperty]
        private Calendar.CalendarViewModel _calendar;

        [ObservableProperty]
        private TaskListViewModel _taskList;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

        [ObservableProperty]
        private bool _isTaskDetailVisible = false;

        [ObservableProperty]
        private TaskDetailViewModel? _currentTaskDetail;

        [ObservableProperty]
        private bool _isNewTaskPopupVisible = false;

        [ObservableProperty]
        private NewTaskPopupViewModel? _newTaskPopup;

        [ObservableProperty]
        private bool _isCreateProjectVisible;

        [ObservableProperty]
        private CreateProjectViewModel? _createProjectVM;

        #endregion

        #region Properties

        public bool IsTopBarVisible => true;

        #endregion

        #region Constructors

        public HomeViewModel()
        {
            // Parameterless constructor for design-time support
            Greeting = "Good day, User";
            _homeMenu = null!;
            _mySummaryPage = null!;
            _projectSummaryPage = null!;
            _calendar = null!;
            _taskList = null!;
            _currentView = null!;
            _authService = null!;
            _timeService = null!;
            _projectTaskRepository = null!;
            _projectRepository = null!;
            _customerRepository = null!;
            _projectTaskModelRepository = null!;
            _appSettingsRepository = null!;
            _staffRepository = null!;
            _taskAssignmentRepository = null!;
            _commentRepository = null!;
            _userRepository = null!;
            _dialogService = null!;
            _loggerFactory = null!;
        }

        public HomeViewModel(HomeMenuViewModel homeMenu,
                             SummaryViewModel mySummary,
                             TasksWidgetViewModel myTasks,
                             PulseViewModel projectPulse,
                             ProjectSummaryViewModel projectSummary,
                             IAuthService authService,
                             ITimeService timeService,
                             IRepository<ProjectTask> projectTaskRepository,
                             IRepository<Project> projectRepository,
                             IRepository<Customer> customerRepository,
                             IRepository<ProjectTask> projectTaskModelRepository,
                             IRepository<AppSetting> appSettingsRepository,
                             IRepository<Employee> staffRepository,
                             IRepository<TaskAssignment> taskAssignmentRepository,
                             IRepository<TaskComment> commentRepository,
                             IRepository<User> userRepository,
                             IDialogService dialogService,
                             ILoggerFactory loggerFactory)
        {
            _authService = authService;
            _currentView = null!; // Silence warning, set in Initialize()
            _timeService = timeService;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _customerRepository = customerRepository;
            _projectTaskModelRepository = projectTaskModelRepository;
            _appSettingsRepository = appSettingsRepository;
            _staffRepository = staffRepository;
            _taskAssignmentRepository = taskAssignmentRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _dialogService = dialogService;
            _loggerFactory = loggerFactory;

            HomeMenu = homeMenu;
            
            // Initialize Pages
            MySummaryPage = new MySummaryPageViewModel(mySummary, myTasks, projectPulse);
            ProjectSummaryPage = new ProjectSummaryPageViewModel(projectSummary, new TeamSummaryViewModel());
            Calendar = new Calendar.CalendarViewModel(_projectTaskRepository, _projectRepository, _authService);
            TaskList = new TaskListViewModel(_projectTaskRepository, _loggerFactory.CreateLogger<TaskListViewModel>());
            
            // Subscribe to selection
            TaskList.TaskSelectionRequested += (s, idString) => 
            {
                if (Guid.TryParse(idString, out var guid))
                    OpenTaskDetail(guid);
            };

            WeakReferenceMessenger.Default.Register<CreateProjectMessage>(this, (r, m) => OpenCreateProject());
            WeakReferenceMessenger.Default.Register<CreateNewTaskMessage>(this, (r, m) => OpenNewTaskPopup());

            HomeMenu.PropertyChanged += HomeMenu_PropertyChanged;

            Initialize();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void OpenTaskDetail(Guid taskId)
        {
            CurrentTaskDetail = new TaskDetailViewModel(_projectTaskRepository, _staffRepository, _taskAssignmentRepository, _commentRepository, _dialogService);
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

        #endregion

        #region Helper Methods

        private void Initialize()
        {
            var now = DateTime.Now;
            Greeting = GetGreeting(now);
            CurrentDate = now.ToString("dd MMMM yyyy");
            
            // Set default view
            UpdateVisibility();
        }

        private void HomeMenu_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HomeMenuViewModel.ActiveTab))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            switch (HomeMenu.ActiveTab)
            {
                case "Portfolio Summary":
                case "Project Summary":
                    CurrentView = ProjectSummaryPage;
                    break;
                case "Team Summary":
                     // Currently mapped to ProjectSummaryPage as well or needs its own?
                     // Based on previous logic, TeamSummary was a separate boolean.
                     // But I put TeamSummary INSIDE ProjectSummaryPage.
                     // IMPORTANT: If they are different tabs, showing BOTH on one page is confusing if they are separate tabs.
                     // The user request said "Reorganizing... MySummary, ProjectSummary, List, Calendar".
                     // It didn't mention Portfolio Summary or Team Summary explicitly as top level folders, but logically they might be.
                     // For now, I'll map Team Summary to ProjectSummaryPage too, or create a specific one.
                     // Actually, in the old logic, IsTeamSummaryVisible was separate.
                     // If I put TeamSummaryViewModel inside ProjectSummaryPageViewModel, then ProjectSummaryPageView shows BOTH.
                     // If the user clicks "Team Summary", they expect to see Team Summary.
                     // Logic: If active tab is Team Summary, maybe show ProjectSummaryPage but scroll to Team Summary? Or just show it.
                     // Let's stick to ProjectSummaryPage for now.
                    CurrentView = ProjectSummaryPage; 
                    break;
                case "Calendar": 
                    CurrentView = Calendar;
                    break;
                case "List":
                    CurrentView = TaskList;
                    break;
                case "My Summary":
                default:
                    CurrentView = MySummaryPage;
                    break;
            }
        }

        private async void CreateNewTask()
        {
            var newTask = new ProjectTask
            {
                Name = "New Task",
                Description = "",
            };

            await _projectTaskRepository.AddAsync(newTask);
            OpenTaskDetail(newTask.Id);
        }

        private string GetGreeting(DateTime time)
        {
            string timeGreeting = time.Hour < 12 ? "Good morning" :
                                  time.Hour < 18 ? "Good afternoon" : "Good evening";

            var userName = _authService.CurrentUser?.DisplayName ?? "User";
            return $"{timeGreeting}, {userName}";
        }

        private void OpenNewTaskPopup()
        {
            NewTaskPopup = new NewTaskPopupViewModel(_projectTaskRepository, _projectRepository, _authService);
            _ = NewTaskPopup.LoadData();

            NewTaskPopup.CloseRequested += (s, e) => CloseNewTaskPopup();
            IsNewTaskPopupVisible = true;
        }

        private void CloseNewTaskPopup()
        {
            IsNewTaskPopupVisible = false;
            NewTaskPopup = null;
        }

        private void OpenCreateProject()
        {
            CreateProjectVM = new CreateProjectViewModel(_projectRepository, _customerRepository, _projectTaskModelRepository, _appSettingsRepository, _staffRepository);
            CreateProjectVM.CloseRequested += (s, e) => CloseCreateProject();
            CreateProjectVM.ProjectCreated += ProjectCreatedHandler;
            IsCreateProjectVisible = true;
        }

        private void CloseCreateProject()
        {
            IsCreateProjectVisible = false;
            CreateProjectVM = null;
        }

        private void ProjectCreatedHandler(object? sender, Guid projectId)
        {
        }

        #endregion
    }
}