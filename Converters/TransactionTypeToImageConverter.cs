using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TindaKo.Converters
{
    public class TransactionTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value as string;

            return type switch
            {
                "Cash" => "peso.png",
                "Credit" => "frown.png",
                _ => "peso.png" // Optional fallback image
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
