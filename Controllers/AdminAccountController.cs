using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;

namespace TheQuel.Controllers
{
    [Route("admin")]
    public class AdminAccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        
        public AdminAccountController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // TODO: In production, implement rate limiting for failed login attempts
            // LOG: Attempt to login as admin with email: {model.Email}
            
            var user = await _authService.LoginAsync(model.Email, model.Password);
            
            if (user == null || user.Role != UserRole.Admin)
            {
                // LOG: Failed login attempt for admin with email: {model.Email}
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Only administrators can login here.");
                return View(model);
            }
            
            // LOG: Successful login for admin {user.Email}
            await SignInAdmin(user);
            
            // Add a success message for the admin
            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}! You are now logged in.";
            
            return RedirectToLocal(returnUrl);
        }
        
        [HttpGet("register")]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            return View(new AdminRegisterViewModel());
        }
        
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(AdminRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Verify admin registration code
            // In a real application, this would be stored securely, not hardcoded
            const string ValidRegistrationCode = "ADMIN-123456";
            
            if (model.RegistrationCode != ValidRegistrationCode)
            {
                ModelState.AddModelError("RegistrationCode", "Invalid registration code.");
                return View(model);
            }
            
            try
            {
                var user = await _userService.CreateUserAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Password,
                    UserRole.Admin,
                    model.Address,
                    model.PhoneNumber
                );
                
                TempData["SuccessMessage"] = "New admin user created successfully.";
                return RedirectToAction("Dashboard", "Admin");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "AdminAccount");
        }
        
        private async Task SignInAdmin(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("IsAdmin", "true")
            };
            
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
        
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Dashboard", "Admin");
            }
        }
    }
} 