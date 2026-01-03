using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _companyName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string? _phone;

        [ObservableProperty]
        private UserRole _userRole;

        [ObservableProperty]
        private string? _errorMessage;

        public IEnumerable<UserRole> AvailableRoles => Enum.GetValues<UserRole>();

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
            UserRole = UserRole.Guest; // Default role
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Please fill in all required fields.";
                return;
            }

            var user = new User
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone, 
                UserRole = UserRole
            };

            var success = await _authService.RegisterAsync(user);
            if (!success)
            {
                ErrorMessage = "Registration failed. User may already exist.";
            }
            else
            {
                ErrorMessage = null;
                // Navigation to Main logic would go here
            }
        }
    }
}
