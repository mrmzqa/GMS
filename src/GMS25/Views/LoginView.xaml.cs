using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using GMS25.ViewModels;

namespace GMS25.Views
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
                // Handle password box binding
                PasswordBox.PasswordChanged += (s, args) =>
                {
                    viewModel.Password = PasswordBox.Password;
                };
                
                // Clear password on load
                PasswordBox.Clear();
                viewModel.Password = string.Empty;
                viewModel.ErrorMessage = string.Empty;
            }
        }
    }
}