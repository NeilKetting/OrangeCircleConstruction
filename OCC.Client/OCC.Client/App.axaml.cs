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
using OCC.Client.ViewModels.Shared;
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
            // Repositories
            services.AddSingleton<IRepository<User>, MockUserRepository>();

            // Services
            services.AddSingleton<IAuthService, MockAuthService>();
            services.AddSingleton<IRepository<TaskItem>, MockTaskItemRepository>();
            services.AddSingleton<IRepository<Project>, MockProjectRepository>();
            services.AddSingleton<IRepository<StaffMember>, MockStaffRepository>();
            services.AddSingleton<IRepository<AttendanceRecord>, MockAttendanceRepository>();
            services.AddSingleton<IRepository<TimeRecord>, MockTimeRecordRepository>();
            services.AddSingleton<ITimeService, TimeService>();
            services.AddSingleton<INotificationService, MockNotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SidebarViewModel>();
            services.AddTransient<TopBarViewModel>();
            services.AddTransient<SummaryViewModel>();
            services.AddTransient<TasksWidgetViewModel>();
            services.AddTransient<PulseViewModel>();
            services.AddTransient<ProjectSummaryViewModel>();
            services.AddTransient<TaskListViewModel>();
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