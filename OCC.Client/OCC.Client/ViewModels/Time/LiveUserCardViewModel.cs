using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using System;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels;

namespace OCC.Client.ViewModels.Time
{
    public partial class LiveUserCardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _employeeId;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _clockInTimeDisplay = "--:--";

        [ObservableProperty]
        private bool _isPresent;

        [ObservableProperty]
        private IBrush _borderBrush = Brushes.Transparent;

        [ObservableProperty]
        private IBrush _backgroundBrush = Brushes.White;

        [ObservableProperty]
        private string _branch = string.Empty;

        [ObservableProperty]
        private string _clockOutTimeDisplay = "--:--";

        public LiveUserCardViewModel()
        {
            // Default State
            UpdateVisuals();
        }

        public void SetStatus(bool isPresent, TimeSpan? clockInTime, TimeSpan? clockOutTime, string branch)
        {
            IsPresent = isPresent;
            Branch = branch;
            
            ClockInTimeDisplay = isPresent && clockInTime.HasValue 
                ? clockInTime.Value.ToString(@"hh\:mm") 
                : "--:--";

            ClockOutTimeDisplay = clockOutTime.HasValue
                ? clockOutTime.Value.ToString(@"hh\:mm")
                : "--:--";
            
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (IsPresent)
            {
                // Green border for present? Or just standard?
                // Request said: "Yellow border on the card" if NOT present.
                BorderBrush = Brushes.LightGray; // Normal
            }
            else
            {
                // Yellow Border for Absent
                BorderBrush = new SolidColorBrush(Color.Parse("#F59E0B")); // Amber-500
            }
        }
    }
}
