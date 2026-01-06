using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Client.ViewModels.Messages;
using OCC.Shared.Models;
using OCC.Client.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCC.Client.ViewModels.Shared
{
    public partial class SidebarViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IAuthService _authService;
        private readonly IUpdateService _updateService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<Project> _projectRepository;
        private readonly IPermissionService _permissionService;
        private List<Project> _allProjects = new();

        #endregion

        #region Observables

        [ObservableProperty]
        private bool _isCollapsed;

        [ObservableProperty]
        private string _activeSection = Infrastructure.NavigationRoutes.Home;

        [ObservableProperty]
        private string _userEmail = string.Empty;

        [ObservableProperty]
        private string _userInitials = string.Empty;

        [ObservableProperty]
        private string _lastActionMessage = string.Empty;

        [ObservableProperty]
        private string _appVersion = string.Empty;

        [ObservableProperty]
        private bool _isQuickActionsOpen;

        [ObservableProperty]
        private bool _isSettingsOpen;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private string _projectSearchText = string.Empty;

        [ObservableProperty]
        private bool _isProjectsExpanded = true;

        public bool CanManageUsers => _permissionService.CanAccess("UserManagement");
        public bool CanViewStaff => _permissionService.CanAccess(Infrastructure.NavigationRoutes.StaffManagement);

        #endregion

        #region Constructors

        public SidebarViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public SidebarViewModel(IAuthService authService, IUpdateService updateService, IServiceProvider serviceProvider, IRepository<Project> projectRepository, IPermissionService permissionService)
        {
            _authService = authService;
            _updateService = updateService;
            _serviceProvider = serviceProvider;
            _projectRepository = projectRepository;
            _permissionService = permissionService;

            AppVersion = $"v{_updateService.CurrentVersion}";

            if (_authService.CurrentUser != null)
            {
                UserEmail = _authService.CurrentUser.Email;
                UserInitials = GetInitials(_authService.CurrentUser.DisplayName);
            }

            // Register for created messages
            WeakReferenceMessenger.Default.Register<ProjectCreatedMessage>(this, (r, m) =>
            {
                _allProjects.Add(m.Value);
                FilterProjects();
                
                // Auto-navigate to the new project
                NavigateToProject(m.Value);
            });

            // Register for deleted messages
            WeakReferenceMessenger.Default.Register<ProjectDeletedMessage>(this, (r, m) =>
            {
                var project = _allProjects.FirstOrDefault(p => p.Id == m.Value);
                if (project != null)
                {
                    _allProjects.Remove(project);
                    FilterProjects();
                }
                
                // If we were on Portfolio, switch to Home
                if (ActiveSection == Infrastructure.NavigationRoutes.Projects)
                {
                    Navigate(Infrastructure.NavigationRoutes.Home);
                }
            });
            
            // Register for update messages
            WeakReferenceMessenger.Default.Register<ProjectUpdatedMessage>(this, (r, m) =>
            {
                LoadProjects();
            });

            LoadProjects();
        }

        #endregion

        #region Commands

        [RelayCommand]
        public void ToggleProjects()
        {
            IsProjectsExpanded = !IsProjectsExpanded;
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task CheckForUpdates()
        {
            LastActionMessage = "Checking for updates...";
            var updateInfo = await _updateService.CheckForUpdatesAsync();
            
            if (updateInfo != null)
            {
                LastActionMessage = "Update available!";
                
                // Show the update dialog on the UI thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                {
                    var dialog = new Views.Shared.UpdateDialogView();
                    dialog.DataContext = new ViewModels.Shared.UpdateDialogViewModel(_updateService, updateInfo, () => dialog.Close());
                    
                    if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null) 
                    {
                        dialog.ShowDialog(desktop.MainWindow);
                    }
                });
            }
            else
            {
                LastActionMessage = "You are up to date.";
            }
        }

        [RelayCommand]
        public void ToggleCollapse()
        {
            IsCollapsed = !IsCollapsed;
        }

        [RelayCommand]
        private void Navigate(string section)
        {
            ActiveSection = section;
            
            // Sync with TopBar tabs
            switch (section)
            {
                case Infrastructure.NavigationRoutes.Home:
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("My Summary"));
                    break;
                case Infrastructure.NavigationRoutes.Time:
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Time"));
                    break;
                case Infrastructure.NavigationRoutes.Projects:
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Projects"));
                    break;
                case Infrastructure.NavigationRoutes.StaffManagement:
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Team"));
                    break;
                case Infrastructure.NavigationRoutes.Notifications:
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Notifications"));
                    break;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.LogoutAsync();
            var loginVm = _serviceProvider.GetRequiredService<LoginViewModel>();
            WeakReferenceMessenger.Default.Send(new NavigationMessage(loginVm));
        }

        [RelayCommand]
        private void Settings() { }

        [RelayCommand]
        private void ToggleTheme() { }

        [RelayCommand]
        private void AccountBilling() { }

        [RelayCommand]
        private void UsersTeams() 
        {
             IsQuickActionsOpen = false;
             IsSettingsOpen = false;
             ActiveSection = "UserManagement";
             LastActionMessage = "Navigating to Manage Users";
        }

        [RelayCommand]
        private void Security() { }

        [RelayCommand]
        private void Integrations() { }

        [RelayCommand]
        private void Alerts() { }

        [RelayCommand]
        private void AuditLog() { }

        [RelayCommand]
        private void AccountExport() { }

        [RelayCommand]
        private void MyProfile() 
        { 
             IsQuickActionsOpen = false; 
             IsSettingsOpen = false;
             ActiveSection = "MyProfile";
        }

        [RelayCommand]
        private void OpenWorkHours()
        {
            IsQuickActionsOpen = false;
            IsSettingsOpen = false;

            // Resolve ViewModel and open
            // Since this is a Popup, we might need to send a message to MainViewModel or similar to overlay it.
            // Or if we are using a dialog service.
            // For now, let's assume we can send a message to open it.
            var vm = new WorkHoursPopupViewModel((AppDbContext)_serviceProvider.GetRequiredService<AppDbContext>());
            WeakReferenceMessenger.Default.Send(new OpenWorkHoursMessage(vm));
        }

        [RelayCommand]
        public void NewTask() 
        { 
            IsQuickActionsOpen = false;
            LastActionMessage = "Action Triggered: New Task (t)";
            WeakReferenceMessenger.Default.Send(new CreateNewTaskMessage());
        }

        [RelayCommand]
        public void NewProject() 
        { 
            IsQuickActionsOpen = false;
            LastActionMessage = "Action Triggered: New Project (p)";
            WeakReferenceMessenger.Default.Send(new CreateProjectMessage());
        }

        [RelayCommand]
        public void NewMeeting() 
        { 
            LastActionMessage = "Action Triggered: New Meeting (m)";
        }

        [RelayCommand]
        public void InviteUser() 
        { 
            LastActionMessage = "Action Triggered: Invite User (i)";
        }

        [RelayCommand]
        public void NewTeamMember() 
        { 
            LastActionMessage = "Action Triggered: New Team Member";
        }

        [RelayCommand]
        private void NavigateToProject(Project project)
        {
            Navigate(Infrastructure.NavigationRoutes.Projects);
            WeakReferenceMessenger.Default.Send(new ProjectSelectedMessage(project));
            LastActionMessage = $"Navigated to Project: {project.Name}";
        }

        #endregion

        #region Methods

        private async void LoadProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            _allProjects = projects.ToList();
            FilterProjects();
        }

        partial void OnProjectSearchTextChanged(string value)
        {
            FilterProjects();
        }

        private void FilterProjects()
        {
            Projects.Clear();
            var filtered = string.IsNullOrWhiteSpace(ProjectSearchText)
                ? _allProjects
                : _allProjects.Where(p => p.Name.Contains(ProjectSearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var p in filtered)
            {
                Projects.Add(p);
            }
        }

        #endregion

        #region Helper Methods

        private string GetInitials(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "U";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        #endregion
    }
}
