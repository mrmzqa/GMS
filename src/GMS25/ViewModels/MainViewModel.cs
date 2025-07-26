using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfPosApp.Services;
using WpfPosApp.Views;

namespace WpfPosApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly LoginView _loginView;
        private readonly HomeView _homeView;
        private readonly ProductsView _productsView;
        private readonly CartView _cartView;
        private readonly OrdersView _ordersView;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private string _title = "POS System";

        public MainViewModel(
            IAuthService authService, 
            LoginView loginView, 
            HomeView homeView,
            ProductsView productsView,
            CartView cartView,
            OrdersView ordersView)
        {
            _authService = authService;
            _loginView = loginView;
            _homeView = homeView;
            _productsView = productsView;
            _cartView = cartView;
            _ordersView = ordersView;

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
            Title = _authService.IsLoggedIn ? $"POS System - {_authService.CurrentUser.Username}" : "POS System";
        }

        [RelayCommand]
        private void NavigateToProducts()
        {
            CurrentView = _productsView;
        }

        [RelayCommand]
        private void NavigateToCart()
        {
            CurrentView = _cartView;
        }

        [RelayCommand]
        private void NavigateToOrders()
        {
            CurrentView = _ordersView;
        }

        [RelayCommand]
        private void NavigateToHome()
        {
            CurrentView = _homeView;
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
        }
    }
}