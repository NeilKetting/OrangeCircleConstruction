using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.Services;
using OCC.Client.Services.ApiServices;
using OCC.Client.Services.Infrastructure; // Added
using OCC.Client.Services.Interfaces; // Added
using OCC.Client.ViewModels.Core; // Added for ViewModelBase/Core VMs
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.HealthSafety;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Home.Calendar;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Login; // Added
using OCC.Client.ViewModels.Notifications; // Added
using OCC.Client.ViewModels.Orders;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Projects.Shared;
using OCC.Client.ViewModels.Settings;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Time;
using OCC.Client.Views.Core;
using OCC.Shared.Models;
using Serilog;
using System;
using System.Linq;


namespace OCC.Client
{
    public partial class App : Application
    {
        public IServiceProvider? Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database
            services.AddDbContext<Data.AppDbContext>(options => { }, ServiceLifetime.Transient); 

            // Repositories
            // repositories - Specific Repositories for API
            services.AddTransient<IRepository<User>, ApiUserRepository>();
            services.AddTransient<IRepository<Employee>, ApiEmployeeRepository>();
            services.AddTransient<IRepository<Project>, ApiProjectRepository>();
            services.AddTransient<IRepository<ProjectTask>, ApiProjectTaskRepository>();
            services.AddTransient<IRepository<Customer>, ApiCustomerRepository>();
            services.AddTransient<IRepository<TaskAssignment>, ApiTaskAssignmentRepository>();
            services.AddTransient<IRepository<TaskComment>, ApiTaskCommentRepository>();
            services.AddTransient<IRepository<TimeRecord>, ApiTimeRecordRepository>();
            services.AddTransient<IRepository<AttendanceRecord>, ApiAttendanceRecordRepository>();
            services.AddTransient<IRepository<AppSetting>, ApiAppSettingRepository>();
            
            // Teams
            services.AddTransient<IRepository<Team>, ApiTeamRepository>();
            services.AddTransient<IRepository<TeamMember>, ApiTeamMemberRepository>();
            
            // Leave & Holidays
            services.AddTransient<IRepository<LeaveRequest>, ApiLeaveRequestRepository>();
            services.AddTransient<IRepository<PublicHoliday>, ApiPublicHolidayRepository>();
            services.AddTransient<IRepository<OvertimeRequest>, ApiOvertimeRequestRepository>();

            // Fallback for any other type not explicitly mapped (e.g. TimeRecord) - though unlikely to be used if we covered main ones
            // services.AddTransient(typeof(IRepository<>), typeof(SqlRepository<>));

             // services.AddSingleton<ITimeService, TimeService>();
             services.AddSingleton<ITimeService, TimeService>();
             
             // Auth Services
             services.AddSingleton<ApiAuthService>();
             services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<ApiAuthService>());

            services.AddSingleton<INotificationService, ApiNotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<IExportService, ExportService>();
            services.AddSingleton<SignalRNotificationService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<LocalSettingsService>();
            services.AddSingleton(ConnectionSettings.Instance);
            services.AddHttpClient<ILeaveService, LeaveService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IOrderService, OrderService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddHttpClient<IInventoryService, InventoryService>(client => client.BaseAddress = new Uri(ConnectionSettings.Instance.ApiBaseUrl));
            services.AddSingleton<IDialogService, DialogService>();

            // Logging
            services.AddLogging(l => l.AddSerilog());

            // ViewModels

            // Core
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddSingleton<SideMenuViewModel>();

            // Login and Registration

            services.AddTransient<RegisterViewModel>();
            services.AddTransient<LoginViewModel>();
            
            // Home
            services.AddTransient<HomeMenuViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddSingleton<NotificationViewModel>();
            
            // Project
            
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ProjectMainMenuViewModel>();



            services.AddTransient<ProjectSummaryViewModel>();
            services.AddTransient<TaskListViewModel>();
            services.AddTransient<ProjectListViewModel>();
            services.AddTransient<ProjectsListViewModel>();

            services.AddTransient<ProjectGanttViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<ManageUsersViewModel>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<TaskDetailViewModel>(); // If needed
            services.AddTransient<EmployeeManagementViewModel>();
            services.AddTransient<TimeLiveViewModel>();
            services.AddTransient<TimeMenuViewModel>();
            services.AddTransient<TimeAttendanceViewModel>();
            services.AddTransient<RollCallViewModel>(); // Added
            services.AddTransient<ClockOutViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<LeaveApplicationViewModel>();
            services.AddTransient<LeaveApprovalViewModel>();
            services.AddTransient<OvertimeViewModel>();
            services.AddTransient<OvertimeApprovalViewModel>();
            services.AddTransient<CalendarViewModel>();
            // services.AddTransient<TeamsViewModel>(); // Removed
            services.AddTransient<TeamManagementViewModel>();
            services.AddTransient<TeamDetailViewModel>();
            services.AddTransient<ProfileViewModel>();
            
            // Health Safety
            services.AddTransient<HealthSafetyViewModel>();
            services.AddTransient<HealthSafetyDashboardViewModel>();

            // Orders
            services.AddTransient<OrderViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<OrderListViewModel>();
            services.AddTransient<OrderDashboardViewModel>();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}