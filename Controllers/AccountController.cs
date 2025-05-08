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
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        
        public AccountController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _authService.LoginAsync(model.Email, model.Password);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            
            // If trying to access admin area without being admin
            if (returnUrl != null && returnUrl.StartsWith("/admin") && user.Role != UserRole.Admin)
            {
                ModelState.AddModelError(string.Empty, "You do not have permission to access the admin area.");
                return View(model);
            }
            
            await SignInUser(user);
            
            TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}! You are now logged in.";
            
            if (user.Role == UserRole.HomeOwner)
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Dashboard", "User");
                }
            }
            else if (user.Role == UserRole.Staff)
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Dashboard", "Staff");
                }
            }
            else if (user.Role == UserRole.Admin)
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            
            return RedirectToLocal(returnUrl);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var user = await _authService.RegisterAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Password,
                    UserRole.HomeOwner, // Regular registration is for homeowners
                    model.Address,
                    model.PhoneNumber
                );
                
                await SignInUser(user);
                
                TempData["SuccessMessage"] = "Registration successful! Welcome to TheQuel.";
                return RedirectToAction("Dashboard", "User");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
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
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 