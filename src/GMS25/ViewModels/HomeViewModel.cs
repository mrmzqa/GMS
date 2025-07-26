
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMS25.Services;

namespace GMS25.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        public HomeViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private void Logout()
        {
            _authService.Logout();
        }
    }
}