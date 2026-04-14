using CommunityToolkit.Mvvm.ComponentModel;

namespace OCC.WpfClient.Infrastructure
{
    /// <summary>
    /// Base class for all ViewModels in the WPF application.
    /// Provides common observable properties like IsBusy and Title.
    /// </summary>
    public abstract partial class ViewModelBase : ObservableValidator
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText = "Please wait...";

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isActiveHub;
    }
}
