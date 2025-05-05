using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TheQuel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPropertyService _propertyService;
        private readonly IUserService _userService;

        public HomeController(
            ILogger<HomeController> logger,
            IPropertyService propertyService,
            IUserService userService)
        {
            _logger = logger;
            _propertyService = propertyService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // If user is authenticated and is a homeowner, redirect to dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("HomeOwner"))
            {
                return RedirectToAction("Dashboard", "User");
            }

            var model = new HomeViewModel
            {
                TotalProperties = await _propertyService.GetAllPropertiesAsync().ContinueWith(t => t.Result.Count()),
                AvailableProperties = await _propertyService.GetAvailablePropertiesAsync().ContinueWith(t => t.Result.Count()),
                TotalHomeowners = await _userService.GetUsersByRoleAsync(UserRole.HomeOwner).ContinueWith(t => t.Result.Count())
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
