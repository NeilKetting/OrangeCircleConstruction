using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Home.Calendar
{
    public partial class CalendarDayViewModel : ViewModelBase
    {
        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private int _dayNumber;

        [ObservableProperty]
        private bool _isCurrentMonth;

        [ObservableProperty]
        private bool _isToday;

        public double Opacity => IsCurrentMonth ? 1.0 : 0.3;

        public ObservableCollection<CalendarTaskViewModel> Tasks { get; } = new();

        public CalendarDayViewModel(DateTime date, bool isCurrentMonth)
        {
            Date = date;
            DayNumber = date.Day;
            IsCurrentMonth = isCurrentMonth;
            IsToday = date.Date == DateTime.Today;
        }
    }

    public enum CalendarTaskSpanType
    {
        Single, 
        Start, 
        Middle, 
        End
    }

    public partial class CalendarTaskViewModel : ViewModelBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#3B82F6"; 
        
        [ObservableProperty]
        private int _visualSlotIndex;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CornerRadius))]
        [NotifyPropertyChangedFor(nameof(Margin))]
        private CalendarTaskSpanType _spanType;

        public bool IsCompleted { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // 24px height + 2px margin = 26px per slot
        public Avalonia.Thickness Margin => new Avalonia.Thickness(0, VisualSlotIndex * 26, 0, 0);

        public Avalonia.CornerRadius CornerRadius => SpanType switch
        {
            CalendarTaskSpanType.Single => new Avalonia.CornerRadius(4),
            CalendarTaskSpanType.Start => new Avalonia.CornerRadius(4, 0, 0, 4),
            CalendarTaskSpanType.Middle => new Avalonia.CornerRadius(0),
            CalendarTaskSpanType.End => new Avalonia.CornerRadius(0, 4, 4, 0),
            _ => new Avalonia.CornerRadius(4)
        };
    }
}
