using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Client.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OCC.Client.ViewModels.Shared
{
    public partial class SidebarViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IUpdateService _updateService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private bool _isCollapsed;

        [ObservableProperty]
        private string _activeSection = "Home";

        [ObservableProperty]
        private string _userEmail = string.Empty;

        [ObservableProperty]
        private string _userInitials = string.Empty;

        [ObservableProperty]
        private string _lastActionMessage = string.Empty;

        [ObservableProperty]
        private string _appVersion = string.Empty;

        public SidebarViewModel(IAuthService authService, IUpdateService updateService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _updateService = updateService;
            _serviceProvider = serviceProvider;

            AppVersion = $"v{_updateService.CurrentVersion}";

            if (_authService.CurrentUser != null)
            {
                UserEmail = _authService.CurrentUser.Email;
                UserInitials = GetInitials(_authService.CurrentUser.DisplayName);
            }
        }

        [RelayCommand]
        public async System.Threading.Tasks.Task CheckForUpdates()
        {
            LastActionMessage = "Checking for updates...";
            var hasUpdate = await _updateService.CheckForUpdatesAsync();
            if (hasUpdate)
            {
                LastActionMessage = "Update available! Installing...";
                await _updateService.DownloadAndInstallUpdateAsync();
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
                case "Home":
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("My Summary"));
                    break;
                case "Time":
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Time"));
                    break;
                case "Portfolio":
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Projects"));
                    break;
                case "Team":
                    WeakReferenceMessenger.Default.Send(new SwitchTabMessage("Team"));
                    break;
                case "Notifications":
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
        private void UsersTeams() { }

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
        private void MyProfile() { }

        [RelayCommand]
        private void MyWorkspaces() { }

        [RelayCommand]
        public void NewTask() 
        { 
            LastActionMessage = "Action Triggered: New Task (t)";
            Navigate("Home");
            WeakReferenceMessenger.Default.Send(new SwitchTabMessage("List"));
        }

        [RelayCommand]
        public void NewProject() 
        { 
            LastActionMessage = "Action Triggered: New Project (p)";
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

        private string GetInitials(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "U";
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }
    }
}
