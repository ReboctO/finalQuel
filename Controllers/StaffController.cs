using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPropertyService _propertyService;
        
        public StaffController(IUserService userService, IPropertyService propertyService)
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
            
            var dashboardViewModel = new StaffDashboardViewModel
            {
                User = user,
                // You can add more properties here to display on the dashboard
            };
            
            return View(dashboardViewModel);
        }
        
        // Add other staff-specific actions here
    }
} 