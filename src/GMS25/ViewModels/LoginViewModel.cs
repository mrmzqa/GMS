using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMS25.Services;
using System.Threading.Tasks;
using System.Windows;

namespace GMS25.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoggingIn = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password";
                return;
            }

            // Disable login button while processing
            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            // Attempt login asynchronously
            bool loginSuccessful = await _authService.LoginAsync(Username, Password);

            // Update UI based on login success or failure
            IsLoggingIn = false;

            if (loginSuccessful)
            {
                // Handle successful login (e.g., navigate to the main app view)
                MessageBox.Show("Login successful!");
            }
            else
            {
                // Handle failed login
                ErrorMessage = "Invalid username or password";
                Password = string.Empty;  // Clear the password field
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
            MessageBox.Show("You have logged out");
        }
    }
}
