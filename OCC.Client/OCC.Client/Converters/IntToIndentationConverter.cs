using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class IntToIndentationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int indentLevel)
            {
                double multiplier = 20.0;
                if (parameter is string paramStr && double.TryParse(paramStr, out var p))
                {
                    multiplier = p;
                }
                
                // Indent only the left side
                return new Thickness(indentLevel * multiplier, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
