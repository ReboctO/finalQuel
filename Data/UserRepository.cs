using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<IEnumerable<User>> GetHomeownersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.HomeOwner)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }
        
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }
        
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
        {
            return await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission)
                .ToListAsync();
        }
        
        public async Task AddPermissionToUserAsync(int userId, Permission permission)
        {
            // Check if user already has this permission
            bool hasPermission = await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.Permission == permission);
                
            if (!hasPermission)
            {
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    Permission = permission
                };
                
                await _context.UserPermissions.AddAsync(userPermission);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task RemovePermissionFromUserAsync(int userId, Permission permission)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission == permission);
                
            if (userPermission != null)
            {
                _context.UserPermissions.Remove(userPermission);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<bool> UserHasPermissionAsync(int userId, Permission permission)
        {
            // First get the user to check their role
            var user = await _context.Users.FindAsync(userId);
            
            // Admins have all permissions by default
            if (user != null && user.Role == UserRole.Admin)
            {
                return true;
            }
            
            // Otherwise check specific permissions
            return await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.Permission == permission);
        }
    }
} 