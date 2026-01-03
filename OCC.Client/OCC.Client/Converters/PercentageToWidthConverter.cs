using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class PercentageToWidthConverter : IMultiValueConverter
    {
        public static readonly PercentageToWidthConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2 || 
                values[0] is not double percentage || 
                values[1] is not double totalWidth)
            {
                return 0.0;
            }

            // prevent infinite or NaN
            if (double.IsNaN(totalWidth) || double.IsInfinity(totalWidth)) return 0.0;

            // percentage is 0-100
            double factor = Math.Clamp(percentage / 100.0, 0.0, 1.0);
            return factor * totalWidth;
        }
    }
}
