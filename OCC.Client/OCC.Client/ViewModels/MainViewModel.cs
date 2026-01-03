using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Messages;
using System;
using OCC.Client.ViewModels.Home;

namespace OCC.Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IRecipient<NavigationMessage>
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ViewModelBase _currentViewModel;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _currentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Receive(NavigationMessage message)
        {
            CurrentViewModel = message.Value;
        }

        [RelayCommand]
        public void NavigateToLogin() => CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

        [RelayCommand]
        public void NavigateToRegister() => CurrentViewModel = _serviceProvider.GetRequiredService<RegisterViewModel>();

        [RelayCommand]
        public void NavigateToHome() => CurrentViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
    }
}
