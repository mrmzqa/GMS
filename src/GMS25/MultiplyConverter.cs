using System;
using System.Globalization;
using System.Windows.Data;

namespace GMS25
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return 0;

            decimal result = 1;
            foreach (var value in values)
            {
                if (value is decimal decimalValue)
                {
                    result *= decimalValue;
                }
                else if (value is int intValue)
                {
                    result *= intValue;
                }
                else if (value is double doubleValue)
                {
                    result *= (decimal)doubleValue;
                }
                else if (value is string strValue && decimal.TryParse(strValue, out decimal parsedValue))
                {
                    result *= parsedValue;
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}