using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageApp.Services;
using System.Threading.Tasks;

namespace GarageApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public LoginViewModel(AuthenticationService authService)
        {
            _authService = authService;
        }

        [ICommand]
        public async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            var success = await _authService.LoginAsync(Username.Trim(), Password);

            if(success)
            {
                OnLoginSucceeded?.Invoke(this, System.EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }

        public event System.EventHandler? OnLoginSucceeded;
    }
}