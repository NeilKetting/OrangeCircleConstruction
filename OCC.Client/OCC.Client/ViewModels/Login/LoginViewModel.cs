using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;

namespace OCC.Client.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }

            var success = await _authService.LoginAsync(Email, Password);
            if (!success)
            {
                ErrorMessage = "Invalid email or password.";
            }
            else
            {
                ErrorMessage = null;
                var homeViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
                WeakReferenceMessenger.Default.Send(new NavigationMessage(homeViewModel));
            }
        }
    }
}
