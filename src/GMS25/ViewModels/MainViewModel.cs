using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.ViewModels
{
    internal class MainViewModel
    {
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfAuthApp.Services;
using WpfAuthApp.Views;

namespace WpfAuthApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly LoginView _loginView;
        private readonly HomeView _homeView;

        [ObservableProperty]
        private object _currentView;

        public MainViewModel(IAuthService authService, LoginView loginView, HomeView homeView)
        {
            _authService = authService;
            _loginView = loginView;
            _homeView = homeView;

            _authService.Logout(); // Start with logout state
            CurrentView = _loginView;
            
            // Subscribe to authentication state changes
            _authService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IAuthService.IsLoggedIn))
                {
                    UpdateView();
                }
            };
        }

        private void UpdateView()
        {
            CurrentView = _authService.IsLoggedIn ? _homeView : _loginView;
        }
    }
}