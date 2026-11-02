using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CMMS_QRCode_Scanner.Shared.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolVal)
            {
                return !boolVal;
            }
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolVal)
            {
                return !boolVal;
            }
            return false;
        }
    }
}
