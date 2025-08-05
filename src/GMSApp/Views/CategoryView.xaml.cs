using GMSApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace GMSApp.Views
{
    public partial class CategoryView : UserControl
    {
        public CategoryView(CategoryViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}