using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GMS25.Data;
using GMS25.Models;

namespace GMS25.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private User _currentUser;

        public bool IsLoggedIn => _currentUser != null;
        public User CurrentUser => _currentUser;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            _currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            
            return IsLoggedIn;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}
}
