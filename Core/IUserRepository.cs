using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetHomeownersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
        Task AddPermissionToUserAsync(int userId, Permission permission);
        Task RemovePermissionFromUserAsync(int userId, Permission permission);
        Task<bool> UserHasPermissionAsync(int userId, Permission permission);
    }
} 