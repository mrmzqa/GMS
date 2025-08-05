using GMSApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class ProductView : UserControl
    {
        public ProductView(ProductViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}