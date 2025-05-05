using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<User> CreateUserAsync(string firstName, string lastName, string email, string password, UserRole role, string address, string phoneNumber);
        Task<User> UpdateUserAsync(int id, string firstName, string lastName, string email, string phoneNumber, string address, UserRole role, bool isActive);
        
        // Permission management
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
        Task AddPermissionToUserAsync(int userId, Permission permission);
        Task RemovePermissionFromUserAsync(int userId, Permission permission);
        Task<bool> UserHasPermissionAsync(int userId, Permission permission);
        Task<Dictionary<Permission, bool>> GetAllPermissionsWithStatusAsync(int userId);
    }
} 