using System.Windows;
using System.Windows.Controls;
using WpfPosApp.ViewModels;

namespace WpfPosApp.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                PasswordBox.PasswordChanged += (s, args) =>
                {
                    viewModel.Password = PasswordBox.Password;
                };
                
                PasswordBox.Clear();
                viewModel.Password = string.Empty;
                viewModel.ErrorMessage = string.Empty;
                PasswordBox.Focus();
            }
        }
    }
}