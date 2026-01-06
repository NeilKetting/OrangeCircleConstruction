using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.Services;
using OCC.Client.ViewModels;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.Views;
using OCC.Shared.Models;
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
            // services.AddDbContext<Data.AppDbContext>(); // Removed duplicate, we configure it below with Transient lifetime

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Data.SqlRepository<>));
            // services.AddSingleton<IRepository<User>, MockUserRepository>(); // Keep mock/specific if SqlRepository generic isn't enough OR use generic

            // Services
            // For AuthService, we might need a real implementation that uses the SqlRepository, or update the mock to use it?
            // The user said "I repositories can save to the db".
            // Let's assume we use the generic repository for everything.
            
            // However, AuthService usually needs specific logic (login). 
            // For now, let's keep MockAuthService but maybe it needs to interact with the DB?
            // Or let's use a real AuthService if we had one? The user only mentioned repositories.
            // Let's swap the repositories first.
            
            // Auth Services
            services.AddSingleton<MockAuthService>();
            services.AddSingleton<ApiAuthService>();
            services.AddSingleton<IAuthService, HybridAuthService>();
            
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
            
            services.AddDbContext<Data.AppDbContext>(options => { }, ServiceLifetime.Transient); 
            services.AddTransient(typeof(IRepository<>), typeof(Data.SqlRepository<>));

            // services.AddSingleton<ITimeService, TimeService>();
             services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<INotificationService, MockNotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddSingleton<SignalRNotificationService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SidebarViewModel>();
            services.AddTransient<TopBarViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddTransient<NotificationViewModel>();
            services.AddTransient<ProjectSummaryViewModel>();
            services.AddTransient<TaskListViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ProjectListViewModel>();
            services.AddTransient<ProjectListViewModel>();
            services.AddTransient<ProjectGanttViewModel>();
            services.AddTransient<ViewModels.Settings.ManageUsersViewModel>();
            services.AddTransient<ViewModels.Settings.AuditLogViewModel>();
            services.AddTransient<TaskDetailViewModel>(); // If needed
            services.AddTransient<EmployeeManagementViewModel>();
            services.AddTransient<ViewModels.Time.TimeViewModel>();
            services.AddTransient<ViewModels.Home.Calendar.CalendarViewModel>();
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