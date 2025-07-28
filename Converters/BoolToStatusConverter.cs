using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TindaKo.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Paid" : "Unpaid";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (string)value == "Paid";
    }

}
