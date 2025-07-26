using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GMS25
{
    /// <summary>
    /// Interaction logic for MultiplyConverter.xaml
    /// </summary>
    public partial class MultiplyConverter : Window
    {
        public MultiplyConverter()
        {
  
using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfPosApp
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is decimal price && values[1] is int quantity)
            {
                return price * quantity;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}