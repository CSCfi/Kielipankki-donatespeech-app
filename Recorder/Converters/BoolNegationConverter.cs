using System;
using System.Globalization;
using Xamarin.Forms;

namespace Recorder.Converters
{
    public class BoolNegationConverter : IValueConverter
    {
        public BoolNegationConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }
    }
}
