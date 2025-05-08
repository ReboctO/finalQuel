using System;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        public async Task<User> RegisterAsync(string firstName, string lastName, string email, string password, UserRole role, string address = "", string phoneNumber = "")
        {
            // Check if email is already registered
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
                Password = HashPassword(password),
                Role = role,
                Address = address ?? string.Empty,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            await _userRepository.AddAsync(user);
            return user;
        }
        
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null || !VerifyPassword(password, user.Password))
            {
                return null;
            }
            
            return user;
        }
        
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null || !VerifyPassword(currentPassword, user.Password))
            {
                return false;
            }
            
            user.Password = HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;
            
            await _userRepository.UpdateAsync(user);
            return true;
        }
        
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}