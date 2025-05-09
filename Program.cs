using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Threading.Tasks;
using TheQuel.Data;

namespace TheQuel
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, serverVersion)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());

            // Register repositories
            builder.Services.AddScoped<TheQuel.Core.IUserRepository, TheQuel.Data.UserRepository>();
            builder.Services.AddScoped<TheQuel.Core.IPropertyRepository, TheQuel.Data.PropertyRepository>();
            builder.Services.AddScoped<TheQuel.Core.IAnnouncementRepository, TheQuel.Data.AnnouncementRepository>();
            builder.Services.AddScoped<TheQuel.Core.IPaymentRepository, TheQuel.Data.PaymentRepository>();
            builder.Services.AddScoped<TheQuel.Core.IFacilityRepository, TheQuel.Data.FacilityRepository>();
            builder.Services.AddScoped<TheQuel.Core.IFacilityReservationRepository, TheQuel.Data.FacilityReservationRepository>();

            // Register unit of work
            builder.Services.AddScoped<TheQuel.Core.IUnitOfWork, TheQuel.Data.UnitOfWork>();

            // Register services
            builder.Services.AddScoped<TheQuel.Services.IAuthService, TheQuel.Services.AuthService>();
            builder.Services.AddScoped<TheQuel.Services.IUserService, TheQuel.Services.UserService>();
            builder.Services.AddScoped<TheQuel.Services.IPropertyService, TheQuel.Services.PropertyService>();
            builder.Services.AddScoped<TheQuel.Services.IAnnouncementService, TheQuel.Services.AnnouncementService>();
            builder.Services.AddScoped<TheQuel.Services.IPaymentService, TheQuel.Services.PaymentService>();
            builder.Services.AddScoped<TheQuel.Services.IFacilityService, TheQuel.Services.FacilityService>();
            builder.Services.AddScoped<TheQuel.Services.IFacilityReservationService, TheQuel.Services.FacilityReservationService>();

            // Add authentication and authorization
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Admin", "Staff"));
                options.AddPolicy("HomeOwnerOnly", policy => policy.RequireRole("HomeOwner"));
            });

            // Set a different port
            builder.WebHost.UseUrls("http://localhost:5050", "https://localhost:5051");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Initialize the database with seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("Ensuring database is created and up to date...");
                    
                    // Make sure database is created
                    context.Database.EnsureCreated();
                    
                   
                    // Ensure Announcements tables exist
                    await DatabaseMigrator.MigrateAnnouncementTablesAsync(app.Services);
                    
                    // Seed admin user
                    Console.WriteLine("Seeding admin user...");
                    await AdminSeeder.SeedAdminUser(app.Services);
                    Console.WriteLine("Database initialization complete.");
                    Console.WriteLine("----------------------------");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                    Console.WriteLine($"Database initialization error: {ex.Message}");
                    Console.WriteLine(ex.ToString());
                }
            }

            await app.RunAsync();
        }
    }
}
