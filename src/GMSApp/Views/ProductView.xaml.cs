using GMSApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GMSApp.Views
{
    public partial class ProductView : Window
    {
        public ProductView(ProductViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}