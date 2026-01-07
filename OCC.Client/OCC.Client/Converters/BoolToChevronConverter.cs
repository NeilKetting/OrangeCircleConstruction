using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using Avalonia;

namespace OCC.Client.Converters
{
    public class BoolToChevronConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool expanded && Application.Current?.Styles.TryGetResource(expanded ? "IconChevronDown" : "IconChevronRight", null, out var resource) == true)
            {
                return resource;
            }
            return null; // Should have fallback data or throw
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
