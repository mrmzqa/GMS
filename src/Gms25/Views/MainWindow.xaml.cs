using System.Windows;
using Gms25.ViewModels;

namespace Gms25.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(UserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, __) => await viewModel.LoadUsersCommand.ExecuteAsync(null);
        }
    }
}