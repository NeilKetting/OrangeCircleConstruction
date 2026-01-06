using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.Messages;
using OCC.Client.Services;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using OCC.Client.ViewModels.Settings;

namespace OCC.Client.ViewModels
{
    public partial class ShellViewModel : ViewModelBase, IRecipient<OpenNotificationsMessage>
    {
        #region Private Members

        private readonly IServiceProvider _serviceProvider;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Observables

        [ObservableProperty]
        private SidebarViewModel _sidebar;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        #endregion

        #region Constructors

        public ShellViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public ShellViewModel(IServiceProvider serviceProvider, SidebarViewModel sidebar, IUpdateService updateService, IPermissionService permissionService)
        {
            _serviceProvider = serviceProvider;
            _permissionService = permissionService;
            Sidebar = sidebar;
            Sidebar.PropertyChanged += Sidebar_PropertyChanged;

            // Default to Home (Dashboard)
            NavigateTo(Infrastructure.NavigationRoutes.Home);

            WeakReferenceMessenger.Default.RegisterAll(this); // Register for messages

            // Check for updates in background, but show UI if found
            Task.Run(async () => 
            {
                var updateInfo = await updateService.CheckForUpdatesAsync();
                if (updateInfo != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                    {
                        var dialog = new Views.Shared.UpdateDialogView();
                        dialog.DataContext = new ViewModels.Shared.UpdateDialogViewModel(updateService, updateInfo, () => dialog.Close());
                        
                        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop) 
                        {
                            dialog.ShowDialog(desktop.MainWindow);
                        }
                    });
                }
            });
        }

        #endregion

        #region Helper Methods

        private void Sidebar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.ActiveSection))
            {
                NavigateTo(Sidebar.ActiveSection);
            }
        }

        private void NavigateTo(string section)
        {
            if (!_permissionService.CanAccess(section))
            {
                // Access Denied - maybe show a notification or just stay put
                // For now, let's redirect to Home if trying to access something forbidden
                if (section != Infrastructure.NavigationRoutes.Home)
                {
                    // Optionally notify user
                    NavigateTo(Infrastructure.NavigationRoutes.Home);
                }
                return;
            }

            switch (section)
            {
                case Infrastructure.NavigationRoutes.Home:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.StaffManagement:
                    CurrentPage = _serviceProvider.GetRequiredService<EmployeeManagementViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Projects:
                    CurrentPage = _serviceProvider.GetRequiredService<ProjectsViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Time:
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Time.TimeViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Calendar: 
                    // Assuming accessing Calendar via Sidebar (if implemented) or other means
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Home.Calendar.CalendarViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Notifications:
                    CurrentPage = _serviceProvider.GetRequiredService<NotificationViewModel>();
                    break;
                case "UserManagement":
                    CurrentPage = _serviceProvider.GetRequiredService<UserManagementViewModel>();
                    break;
                case "MyProfile":
                    CurrentPage = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    break;
                 default:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
            }
        }

        public void Receive(OpenNotificationsMessage message)
        {
            NavigateTo(Infrastructure.NavigationRoutes.Notifications);
        }



        #endregion
    }
}
