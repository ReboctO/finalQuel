using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Services;

namespace TheQuel.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly Permission[] _requiredPermissions;
        
        public RequirePermissionAttribute(params Permission[] permissions)
        {
            _requiredPermissions = permissions;
        }
        
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }
            
            // Get user ID from claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new ForbidResult();
                return;
            }
            
            // Get user role from claims
            var roleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && roleClaim.Value == UserRole.Admin.ToString())
            {
                // Admins have all permissions
                return;
            }
            
            // Get user service from DI
            var userService = context.HttpContext.RequestServices.GetService(typeof(IUserService)) as IUserService;
            if (userService == null)
            {
                context.Result = new ForbidResult();
                return;
            }
            
            // Check if user has at least one of the required permissions
            foreach (var permission in _requiredPermissions)
            {
                if (await userService.UserHasPermissionAsync(userId, permission))
                {
                    // User has at least one required permission
                    return;
                }
            }
            
            // User doesn't have any of the required permissions
            context.Result = new ForbidResult();
        }
    }
} 