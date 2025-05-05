using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Services;

namespace TheQuel
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            // Check if admin exists
            var adminEmail = "admin@thequel.com";
            var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
            
            if (existingAdmin == null)
            {
                try
                {
                    // Create admin user
                    var admin = await authService.RegisterAsync(
                        firstName: "System",
                        lastName: "Administrator",
                        email: adminEmail,
                        password: "Admin123!",
                        role: UserRole.Admin,
                        address: "System Address",
                        phoneNumber: "123-456-7890"
                    );
                    
                    Console.WriteLine($"Admin user created successfully: {admin.Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating admin user: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
} 