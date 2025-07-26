using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.ViewModels
{
    internal class HomeViewModel
    {
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfAuthApp.Services;

namespace WpfAuthApp.ViewModels
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