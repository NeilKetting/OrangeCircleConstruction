using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Messages;
using System;
using OCC.Client.ViewModels.Home;

using OCC.Shared.Models;
using OCC.Client.Services;

namespace OCC.Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IRecipient<NavigationMessage>, IRecipient<OpenProfileMessage>
    {
        #region Private Members

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Observables

        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        [ObservableProperty]
        private bool _isProfileVisible;

        [ObservableProperty]
        private ViewModels.Shared.ProfileViewModel? _currentProfile;

        [ObservableProperty]
        private bool _isChangeEmailVisible;

        [ObservableProperty]
        private ViewModels.Shared.ChangeEmailPopupViewModel? _changeEmailPopup;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            // Parameterless constructor for design-time support                

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _currentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        #endregion

        #region Commands

        [RelayCommand]
        public void NavigateToLogin() => CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

        [RelayCommand]
        public void NavigateToRegister() => CurrentViewModel = _serviceProvider.GetRequiredService<RegisterViewModel>();

        [RelayCommand]
        public void NavigateToHome() => CurrentViewModel = _serviceProvider.GetRequiredService<ShellViewModel>();

        #endregion

        #region Methods

        public void Receive(NavigationMessage message)
        {
            CurrentViewModel = message.Value;
        }

        public void Receive(OpenProfileMessage message)
        {
            var auth = _serviceProvider.GetRequiredService<IAuthService>();
            var projectRepo = _serviceProvider.GetRequiredService<IRepository<Project>>();
            
            CurrentProfile = new ViewModels.Shared.ProfileViewModel(auth, projectRepo);
            CurrentProfile.CloseRequested += (s, e) => 
            {
                IsProfileVisible = false;
                CurrentProfile = null;
            };
            CurrentProfile.ChangeEmailRequested += (s, e) => OpenChangeEmailPopup();
            IsProfileVisible = true;
        }

        #endregion

        #region Helper Methods

        private void OpenChangeEmailPopup()
        {
             ChangeEmailPopup = new ViewModels.Shared.ChangeEmailPopupViewModel();
             ChangeEmailPopup.CloseRequested += (s, e) => 
             {
                 IsChangeEmailVisible = false;
                 ChangeEmailPopup = null;
             };
             ChangeEmailPopup.EmailChanged += (s, newEmail) =>
             {
                 if (CurrentProfile != null)
                 {
                     CurrentProfile.Email = newEmail;
                     // Also update auth service user if possible
                     // But ProfileViewModel Done() handles saving mostly.
                     // The user asked for immediate feedback visually?
                 }
             };
             IsChangeEmailVisible = true;
        }

        #endregion
    }
}
