using GMSApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GMSApp.Views
{
    public partial class CategoryView : Window
    {
        public CategoryView(CategoryViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}