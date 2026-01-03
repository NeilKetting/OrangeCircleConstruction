using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OCC.Client.Converters
{
    public class SidebarConverters
    {
        public static readonly IValueConverter CollapseWidth = 
            new FuncValueConverter<bool, double>(isCollapsed => isCollapsed ? 70.0 : 250.0);

        public static readonly IValueConverter CollapseIcon = 
            new FuncValueConverter<bool, object?>(isCollapsed => 
            {
                var key = isCollapsed ? "IconChevronRight" : "IconChevronLeft";
                if (Application.Current != null && Application.Current.TryGetResource(key, null, out var resource))
                {
                    return resource;
                }
                return null;
            });

        public static readonly IValueConverter ActiveColor = new ActiveColorConverter();
    }

    public class ActiveColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var activeSection = value as string;
            var targetSection = parameter as string;
            return activeSection == targetSection 
                ? new SolidColorBrush(Color.Parse("#2563eb")) 
                : new SolidColorBrush(Color.Parse("#64748b"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
