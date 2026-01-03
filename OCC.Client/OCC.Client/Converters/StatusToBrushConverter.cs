using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Colors matching the design
                return status.ToLower() switch
                {
                    "green" => SolidColorBrush.Parse("#22C55E"), // Green-500
                    "gray" => SolidColorBrush.Parse("#94A3B8"),  // Slate-400
                    "red" => SolidColorBrush.Parse("#EF4444"),   // Red-500
                    "orange" => SolidColorBrush.Parse("#F97316"),// Orange-500
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
