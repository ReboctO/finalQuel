using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string firstName, string lastName, string email, string password, UserRole role, string address = "", string phoneNumber = "");
        Task<User?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
} 