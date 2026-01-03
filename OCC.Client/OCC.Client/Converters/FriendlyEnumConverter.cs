using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace OCC.Client.Converters
{
    public class FriendlyEnumConverter : IValueConverter
    {
        public static readonly FriendlyEnumConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string name = value.ToString() ?? string.Empty;
            
            // Add spaces before capitals (e.g., SiteManager -> Site Manager)
            return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
