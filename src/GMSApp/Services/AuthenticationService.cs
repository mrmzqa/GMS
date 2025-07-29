using GarageApp.Data;
using GarageApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GarageApp.Services
{
    public class AuthenticationService
    {
        private readonly GarageDbContext _dbContext;
        public User? CurrentUser { get; private set; }

        public AuthenticationService(GarageDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var passwordHash = ComputeSha256Hash(password);

            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash && u.IsActive);

            if (user != null)
            {
                CurrentUser = user;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}