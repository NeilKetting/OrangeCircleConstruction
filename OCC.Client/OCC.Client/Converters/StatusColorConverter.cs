using Avalonia.Data.Converters;
using Avalonia.Media;
using OCC.Shared.Models;
using System;
using System.Globalization;

namespace OCC.Client.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is InventoryStatus invStatus && parameter is string param)
            {
                if (param == "Background")
                {
                    return invStatus switch
                    {
                        InventoryStatus.OK => Brushes.LightGreen, // #90EE90
                        InventoryStatus.Low => Brushes.Bisque, // #FFE4C4
                        InventoryStatus.Critical => Brushes.LightCoral, // #F08080
                        _ => Brushes.Transparent
                    };
                }
                else if (param == "Foreground")
                {
                    return invStatus switch
                    {
                        InventoryStatus.OK => Brushes.DarkGreen,
                        InventoryStatus.Low => Brushes.DarkOrange,
                        InventoryStatus.Critical => Brushes.DarkRed,
                        _ => Brushes.Black
                    };
                }
            }
            else if (value is OrderStatus orderStatus) // No parameter needed usually, or default to Background logic if bound to Border
            {
                 // Assuming usage in Border Background mainly
                 return orderStatus switch
                 {
                     OrderStatus.Draft => Brushes.LightGray,
                     OrderStatus.Ordered => Brushes.Orange,
                     OrderStatus.PartialDelivery => Brushes.LightBlue,
                     OrderStatus.Completed => Brushes.LightGreen,
                     OrderStatus.Cancelled => Brushes.LightPink,
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
