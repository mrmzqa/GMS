using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMS25.Services
{
    public interface IAuthService
    {
        bool Login(string username, string password);
        void Logout();
        bool IsLoggedIn { get; }
    }
}

