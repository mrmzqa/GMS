using GMSApp.Commands;
using GMSApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
       
        private bool _isLoggedIn = false; // Default to false


        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new DelegateCommand(ExecuteLogin, CanExecuteLogin);
            LogoutCommand = new DelegateCommand(ExecuteLogout, CanExecuteLogout);
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
                LoginCommand.RaiseCanExecuteChanged();
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand LoginCommand { get; }
        public DelegateCommand LogoutCommand { get; }

        private void ExecuteLogin(object? parameter)
        {
            if (_authService.Login(Username, Password))
            {
                IsLoggedIn = true;
                StatusMessage = $"Welcome, {Username}!";
                Password = string.Empty; // Clear password after login
            }
            else
            {
                StatusMessage = "Invalid username or password!";
            }
        }

        private bool CanExecuteLogin(object? parameter)
        {
            return !IsLoggedIn &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogout(object? parameter)
        {
            _authService.Logout();
            IsLoggedIn = false;
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = "Logged out successfully!";
        }

        private bool CanExecuteLogout(object? parameter)
        {
            return IsLoggedIn;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
