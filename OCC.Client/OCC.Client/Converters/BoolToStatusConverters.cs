using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class BoolToStatusColorConverter : IValueConverter
    {
        public static readonly BoolToStatusColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isApproved && isApproved)
            {
                return Brushes.Green; // Approved
            }
            return Brushes.Orange; // Pending
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToStatusTextConverter : IValueConverter
    {
        public static readonly BoolToStatusTextConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isApproved && isApproved)
            {
                return "APPROVED";
            }
            return "PENDING";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
