using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.Messages;
using System;

namespace OCC.Client.ViewModels
{
    public partial class ShellViewModel : ViewModelBase, IRecipient<OpenNotificationsMessage>
    {
        #region Private Members

        private readonly IServiceProvider _serviceProvider;

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

        public ShellViewModel(IServiceProvider serviceProvider, SidebarViewModel sidebar)
        {
            _serviceProvider = serviceProvider;
            Sidebar = sidebar;

            // Default to Home (Dashboard)
            NavigateTo(Infrastructure.NavigationRoutes.Home);


            WeakReferenceMessenger.Default.RegisterAll(this); // Register for messages
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
