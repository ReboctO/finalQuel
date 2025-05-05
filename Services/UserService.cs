using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _userRepository.GetUsersByRoleAsync(role);
        }
        
        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }
            
            // Check if email is unique if changed
            if (existingUser.Email != user.Email)
            {
                bool isEmailUnique = await _userRepository.IsEmailUniqueAsync(user.Email);
                if (!isEmailUnique)
                {
                    throw new InvalidOperationException("Email is already registered");
                }
            }
            
            // Update user properties
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Address = user.Address;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.Now;
            
            await _userRepository.UpdateAsync(existingUser);
            return existingUser;
        }
        
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                return false;
            }
            
            await _userRepository.RemoveAsync(user);
            return true;
        }
        
        public async Task<User> CreateUserAsync(string firstName, string lastName, string email, string password, UserRole role, string address, string phoneNumber)
        {
            // Check if email is unique
            bool isEmailUnique = await _userRepository.IsEmailUniqueAsync(email);
            if (!isEmailUnique)
            {
                throw new InvalidOperationException("Email is already registered");
            }
            
            // Create new user
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password), // Hash the password
                PhoneNumber = phoneNumber,
                Address = address,
                Role = role,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            await _userRepository.AddAsync(user);
            return user;
        }
        
        public async Task<User> UpdateUserAsync(int id, string firstName, string lastName, string email, string phoneNumber, string address, UserRole role, bool isActive)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {id} not found");
            }
            
            // Check if email is unique if changed
            if (existingUser.Email != email)
            {
                bool isEmailUnique = await _userRepository.IsEmailUniqueAsync(email);
                if (!isEmailUnique)
                {
                    throw new InvalidOperationException("Email is already registered");
                }
            }
            
            // Update user properties
            existingUser.FirstName = firstName;
            existingUser.LastName = lastName;
            existingUser.Email = email;
            existingUser.PhoneNumber = phoneNumber;
            existingUser.Address = address;
            existingUser.Role = role;
            existingUser.IsActive = isActive;
            existingUser.UpdatedAt = DateTime.Now;
            
            await _userRepository.UpdateAsync(existingUser);
            return existingUser;
        }
        
        // Permission management methods
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
        {
            return await _userRepository.GetUserPermissionsAsync(userId);
        }
        
        public async Task AddPermissionToUserAsync(int userId, Permission permission)
        {
            await _userRepository.AddPermissionToUserAsync(userId, permission);
        }
        
        public async Task RemovePermissionFromUserAsync(int userId, Permission permission)
        {
            await _userRepository.RemovePermissionFromUserAsync(userId, permission);
        }
        
        public async Task<bool> UserHasPermissionAsync(int userId, Permission permission)
        {
            return await _userRepository.UserHasPermissionAsync(userId, permission);
        }
        
        public async Task<Dictionary<Permission, bool>> GetAllPermissionsWithStatusAsync(int userId)
        {
            // Get all possible permissions from enum
            var allPermissions = Enum.GetValues(typeof(Permission)).Cast<Permission>();
            
            // Get user's permissions
            var userPermissions = await _userRepository.GetUserPermissionsAsync(userId);
            
            // Create a dictionary with all permissions and whether the user has them
            var result = new Dictionary<Permission, bool>();
            
            foreach (var permission in allPermissions)
            {
                result[permission] = userPermissions.Contains(permission);
            }
            
            return result;
        }
    }
} 