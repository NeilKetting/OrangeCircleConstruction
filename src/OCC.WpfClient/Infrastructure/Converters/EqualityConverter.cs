using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace OCC.WpfClient.Infrastructure.Converters
{
    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return false;
            
            // Handle nulls
            if (values[0] == null && values[1] == null) return true;
            if (values[0] == null || values[1] == null) return false;

            return values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
