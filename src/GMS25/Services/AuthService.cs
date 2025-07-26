using GMS25.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GMS25.Services
{
    public class AuthService : IAuthService
    {
        private bool _isLoggedIn;
        private User _currentUser;

        private readonly List<User> _users = new List<User>
        {
            new User { Username = "admin", Password = "admin123" },
            new User { Username = "user1", Password = "user123" }
        };

        public bool IsLoggedIn => _isLoggedIn;

        public User CurrentUser => _currentUser;

        public event PropertyChangedEventHandler PropertyChanged;

        // Raise PropertyChanged for IsLoggedIn and CurrentUser when their values change
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Asynchronous login method
        public async Task<bool> LoginAsync(string username, string password)
        {
            // Simulate some async login logic
            await Task.Delay(500);  // Simulate a delay

            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                _currentUser = user;
                _isLoggedIn = true;
                OnPropertyChanged(nameof(IsLoggedIn)); // Notify that the login status has changed
                OnPropertyChanged(nameof(CurrentUser)); // Notify that the current user has changed
                return true;
            }

            return false;
        }

        // Logout method to clear session
        public void Logout()
        {
            _currentUser = null;
            _isLoggedIn = false;
            OnPropertyChanged(nameof(IsLoggedIn)); // Notify that the login status has changed
            OnPropertyChanged(nameof(CurrentUser)); // Notify that the current user has changed
        }
    }
}
