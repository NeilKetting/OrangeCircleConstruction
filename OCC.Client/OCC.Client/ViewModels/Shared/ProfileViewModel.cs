using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OCC.Client.ViewModels.Shared
{
    public partial class ProfileViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IAuthService _authService;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<User> _userRepository;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? ChangeEmailRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _initials = string.Empty;

        [ObservableProperty]
        private string _location = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private UserRole _selectedRole;

        #endregion

        #region Properties

        public ObservableCollection<UserRole> Roles { get; } = new(Enum.GetValues<UserRole>());

        #endregion

        #region Constructors

        public ProfileViewModel(IAuthService authService, IRepository<Project> projectRepository)
        {
            _authService = authService;
            _projectRepository = projectRepository;
            // Assuming we'd fetch actual data here
            LoadUserData();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void EditEmail() 
        {
             // Trigger popup via event or messenger
             ChangeEmailRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ResetPassword()
        {
             // Placeholder logic
        }

        [RelayCommand]
        private void Done()
        {
            // Save logic would go here
            if (_authService.CurrentUser != null)
            {
                 // Update local user object for immediate feedback
                 _authService.CurrentUser.Location = Location;
                 _authService.CurrentUser.Phone = Phone;
                 _authService.CurrentUser.UserRole = SelectedRole;
                 // Persist via API...
            }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void LoadUserData()
        {
            var user = _authService.CurrentUser;
            if (user != null)
            {
                FirstName = user.FirstName;
                LastName = user.LastName;
                Email = user.Email;
                Initials = GetInitials(user.DisplayName);
                Phone = user.Phone ?? "";
                Location = user.Location ?? "";
                SelectedRole = user.UserRole;
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
