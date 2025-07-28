using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TindaKo.Converters
{
    public class DueDateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dueDate)
            {
                return dueDate.Date < DateTime.Today ? Colors.Red : Colors.Black;
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
