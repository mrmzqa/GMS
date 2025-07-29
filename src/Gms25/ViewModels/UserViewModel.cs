using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gms25.Models;
using Gms25.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Gms25.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly IGenericRepository<User> _userRepository;

        [ObservableProperty]
        private ObservableCollection<User> users = new();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        public UserViewModel(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
            LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync);
        }

        public IAsyncRelayCommand LoadUsersCommand { get; }
        public IAsyncRelayCommand AddUserCommand { get; }
        public IAsyncRelayCommand UpdateUserCommand { get; }
        public IAsyncRelayCommand DeleteUserCommand { get; }

        private async Task LoadUsersAsync()
        {
            var list = await _userRepository.GetAllAsync();
            Users = new ObservableCollection<User>(list);
        }

        private async Task AddUserAsync()
        {
            var user = new User { Username = Username, Password = Password };
            await _userRepository.AddAsync(user);
            await LoadUsersAsync();
        }

        private async Task UpdateUserAsync()
        {
            if (SelectedUser != null)
            {
                SelectedUser.Username = Username;
                SelectedUser.Password = Password;
                await _userRepository.UpdateAsync(SelectedUser);
                await LoadUsersAsync();
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser != null)
            {
                await _userRepository.DeleteAsync(SelectedUser.Id);
                await LoadUsersAsync();
            }
        }
    }
}