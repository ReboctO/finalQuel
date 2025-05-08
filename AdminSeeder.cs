using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Services;
using TheQuel.Data;
using Microsoft.EntityFrameworkCore;

namespace TheQuel
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                Console.WriteLine("Attempting to create admin user with direct SQL...");
                
                // First, check if the admin user already exists
                const string adminEmail = "admin@thequel.com";
                var adminExists = await dbContext.Users.AnyAsync(u => u.Email == adminEmail);
                
                if (adminExists)
                {
                    // Delete the existing admin user and their permissions
                    Console.WriteLine("Removing existing admin user...");
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM UserPermissions WHERE UserId IN (SELECT Id FROM Users WHERE Email = {0})", 
                        adminEmail);
                    
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM Users WHERE Email = {0}", 
                        adminEmail);
                }
                
                // Generate BCrypt hash for "Admin123!"
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
                
                // Insert the admin user with SQL
                Console.WriteLine("Creating admin user...");
                await dbContext.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO Users (FirstName, LastName, Email, Password, PhoneNumber, Address, Role, CreatedAt, IsActive)
                    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8});
                    SELECT LAST_INSERT_ID();",
                    "System", "Administrator", adminEmail, passwordHash, "123-456-7890", 
                    "System Address", (int)UserRole.Admin, DateTime.Now, true);
                
                // Get the new admin user's ID
                var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
                
                if (adminUser != null)
                {
                    Console.WriteLine($"Admin user created successfully with ID: {adminUser.Id}");
                    
                    // Add all permissions
                    foreach (Permission permission in Enum.GetValues(typeof(Permission)))
                    {
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "INSERT INTO UserPermissions (UserId, Permission) VALUES ({0}, {1})",
                            adminUser.Id, (int)permission);
                    }
                    
                    Console.WriteLine("Admin permissions granted successfully.");
                    Console.WriteLine("------------------------");
                    Console.WriteLine("LOGIN CREDENTIALS:");
                    Console.WriteLine("Email: admin@thequel.com");
                    Console.WriteLine("Password: Admin123!");
                    Console.WriteLine("------------------------");
                }
                else
                {
                    Console.WriteLine("ERROR: Admin user was not created properly!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating admin user: {ex.Message}");
                Console.WriteLine($"Exception details: {ex.ToString()}");
            }
        }
    }
} 