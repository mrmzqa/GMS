using GMS25.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GMS25.Services
{
    public interface IAuthService : INotifyPropertyChanged
    {
        Task<bool> LoginAsync(string username, string password);
        void Logout();
        bool IsLoggedIn { get; }
        User CurrentUser { get; }
    }
}
