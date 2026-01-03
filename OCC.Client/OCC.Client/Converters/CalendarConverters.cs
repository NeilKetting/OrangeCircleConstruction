using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OCC.Client.Converters
{
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth && isCurrentMonth)
            {
                // Active Days: White Background (High contrast)
                return Brushes.White;
            }
            // Inactive Days: Slate/Gray Background (Dimmed)
            return new SolidColorBrush(Color.Parse("#F8FAFC"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth && isCurrentMonth)
            {
                return 1.0;
            }
            return 0.5; // Dimmed text for inactive days
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
