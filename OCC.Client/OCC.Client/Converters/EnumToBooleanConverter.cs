using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using OCC.Shared.Models;

namespace OCC.Client.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public static readonly EnumToBooleanConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            
            // Compare as strings to be safe across types
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                if (parameter is AttendanceStatus status) return status;
                if (Enum.TryParse(parameter.GetType(), parameter.ToString(), out var result))
                {
                    return result;
                }
            }
            return BindingOperations.DoNothing;
        }

        public static readonly IValueConverter NotPresent = new FuncValueConverter<object, bool>(status => 
        {
            if (status is AttendanceStatus s) return s != AttendanceStatus.Present;
            if (status != null && Enum.TryParse<AttendanceStatus>(status.ToString(), out var result))
            {
                return result != AttendanceStatus.Present;
            }
            return false;
        });
    }
}
