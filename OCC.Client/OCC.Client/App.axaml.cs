using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.Services;
using OCC.Client.Services.Interfaces; // Added
using OCC.Client.Services.Infrastructure; // Added
using OCC.Client.Services.ApiServices;
using OCC.Client.ViewModels;
using OCC.Client.ViewModels.Core; // Added for ViewModelBase/Core VMs
using OCC.Client.ViewModels.Login; // Added
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Notifications; // Added
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Shared;
using OCC.Client.Views;
using OCC.Client.Views.Core;
using System;
using System.Linq;
using Serilog;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Calendar;
using OCC.Client.ViewModels.Time;
using OCC.Client.ViewModels.HealthSafety;
using OCC.Client.ViewModels.Settings;
using OCC.Client.ViewModels.Projects.Shared;
using OCC.Client.ViewModels.Projects.Dashboard;


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
            
            // Teams (Local DB for now)
            services.AddTransient<IRepository<Team>, Data.SqlRepository<Team>>();
            services.AddTransient<IRepository<TeamMember>, Data.SqlRepository<TeamMember>>();

            // Fallback for any other type not explicitly mapped (e.g. TimeRecord) - though unlikely to be used if we covered main ones
            // services.AddTransient(typeof(IRepository<>), typeof(SqlRepository<>));
            
            // Services
            // For AuthService, we might need a real implementation that uses the SqlRepository, or update the mock to use it?
            // The user said "I repositories can save to the db".
            // Let's assume we use the generic repository for everything.
            
            // However, AuthService usually needs specific logic (login). 
            // For now, let's keep MockAuthService but maybe it needs to interact with the DB?
            // Or let's use a real AuthService if we had one? The user only mentioned repositories.
            // Let's swap the repositories first.
            
            // Auth Services
            services.AddSingleton<ApiAuthService>();
            services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<ApiAuthService>());
            
            // Re-register specific repositories if they have specific interfaces or we want to force the generic one?
            // The generic registration `typeof(IRepository<>), typeof(SqlRepository<>)` handles any `IRepository<T>` request.
            // So we don't need to manually register each `<TaskItem>`, `<Project>` etc. unless we want singletons (SqlRepo should be Scoped usually).
            
            // IMPORTANT: Avalonia ViewModels are often Transients or Singletons. If Singletons, they can't easily consume Scoped services (DbContext).
            // But we can register DbContext/Repo as Transient or Singleton strictly for this client-side-only usage.
            // Since it's a desktop app with single user, Singleton Context is risky (concurrency) but Transient Context means new connection per usage?
            // Let's try Singleton for DbContext/Repo for simplicity in a desktop app without threading issues initially, OR Transient.
            // EF Core Context is not thread safe.
            // Let's stick to Transient for Repos/Context to be safe, or Scoped if we had a scope.
            // We'll use Transient for now.
            
            // services.AddDbContext<Data.AppDbContext>(options => { }, ServiceLifetime.Transient); 
            // services.AddTransient(typeof(IRepository<>), typeof(Data.SqlRepository<>));

            // services.AddSingleton<ITimeService, TimeService>();
             services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<INotificationService, ApiNotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<SignalRNotificationService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<LocalSettingsService>();
            services.AddSingleton(ConnectionSettings.Instance);

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
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddTransient<NotificationViewModel>();
            
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
            services.AddTransient<LeaveApplicationViewModel>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<TeamsViewModel>();
            services.AddTransient<ProfileViewModel>();
            
            // Health Safety
            services.AddTransient<HealthSafetyViewModel>();
            services.AddTransient<HealthSafetyDashboardViewModel>();
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