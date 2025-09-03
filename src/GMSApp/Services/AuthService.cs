using GMSApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMSApp.Services
{
    public class AuthService
    {
        private readonly List<User> _users = new()
    {
        new User { Username = "admin", Password = "password123" },
        new User { Username = "user", Password = "user123" }
    };

        public User? CurrentUser { get; private set; }

        public bool Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (user != null)
            {
                CurrentUser = user;
                CurrentUser.IsAuthenticated = true;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn() => CurrentUser != null;

    }
}
