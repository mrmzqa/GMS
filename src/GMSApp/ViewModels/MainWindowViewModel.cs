using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMSApp.Services;

namespace GMSApp.ViewModels
{
  
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;

        [ObservableProperty]
        private AppPage currentPage = AppPage.Login;

        public LoginViewModel LoginVM { get; }
        public VehicleViewModel VehicleVM { get; }

        public MainWindowViewModel(AuthenticationService authService, VehicleViewModel vehicleVM, LoginViewModel loginVM)
        {
            _authService = authService;
            VehicleVM = vehicleVM;
            LoginVM = loginVM;

            LoginVM.OnLoginSucceeded += (s, e) =>
            {
                CurrentPage = AppPage.Main;
            };
        }

        [RelayCommand]
        public void Logout()
        {
            _authService.Logout();
            CurrentPage = AppPage.Login;
        }
    }
}