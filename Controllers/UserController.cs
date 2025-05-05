using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "HomeOwner")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPropertyService _propertyService;
        
        public UserController(IUserService userService, IPropertyService propertyService)
        {
            _userService = userService;
            _propertyService = propertyService;
        }
        
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var dashboardViewModel = new DashboardViewModel
            {
                User = user,
                // You can add more properties here to display on the dashboard
            };
            
            return View(dashboardViewModel);
        }
    }
} 