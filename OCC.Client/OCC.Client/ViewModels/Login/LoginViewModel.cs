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
        #region Private Members

        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        private bool _useApi;
        public bool UseApi
        {
            get => _useApi;
            set
            {
                if (SetProperty(ref _useApi, value))
                {
                    Services.ConnectionSettings.Instance.UseApi = value;
                }
            }
        }

        #endregion

        #region Constructors

        public LoginViewModel()
        {
            // Parameterless constructor for design-time support
        }

        public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            
            // Sync with singleton
            UseApi = Services.ConnectionSettings.Instance.UseApi;
            Services.ConnectionSettings.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Services.ConnectionSettings.UseApi))
                {
                    UseApi = Services.ConnectionSettings.Instance.UseApi;
                }
            };
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }

            var (success, errorMessage) = await _authService.LoginAsync(Email, Password);
            if (!success)
            {
                ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Invalid email or password." : errorMessage;
            }
            else
            {
                ErrorMessage = null;
                var shellViewModel = _serviceProvider.GetRequiredService<ShellViewModel>();
                WeakReferenceMessenger.Default.Send(new NavigationMessage(shellViewModel));
            }
        }

        #endregion
    }
}
