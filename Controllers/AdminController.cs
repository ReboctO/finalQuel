using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;
using System.Collections.Generic;
using System;
using TheQuel.Authorization;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPropertyService _propertyService;
        
        public AdminController(IUserService userService, IPropertyService propertyService)
        {
            _userService = userService;
            _propertyService = propertyService;
        }
        
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userService.GetAllUsersAsync().ContinueWith(t => t.Result.Count()),
                TotalHomeowners = await _userService.GetUsersByRoleAsync(UserRole.HomeOwner).ContinueWith(t => t.Result.Count()),
                TotalStaff = await _userService.GetUsersByRoleAsync(UserRole.Staff).ContinueWith(t => t.Result.Count()),
                TotalProperties = await _propertyService.GetAllPropertiesAsync().ContinueWith(t => t.Result.Count()),
                AvailableProperties = await _propertyService.GetAvailablePropertiesAsync().ContinueWith(t => t.Result.Count())
            };
            
            return View(model);
        }
        
        #region User Management
        
        [RequirePermission(Permission.ManageUsers)]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        
        [RequirePermission(Permission.CreateUser)]
        public IActionResult CreateUser()
        {
            return View(new UserCreateViewModel 
            { 
                Role = UserRole.HomeOwner // Set default role
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.CreateUser)]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                await _userService.CreateUserAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Password,
                    model.Role,
                    model.Address,
                    model.PhoneNumber
                );
                
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(UserManagement));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [RequirePermission(Permission.EditUser)]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role,
                IsActive = user.IsActive
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.EditUser)]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                await _userService.UpdateUserAsync(
                    model.Id,
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.PhoneNumber,
                    model.Address,
                    model.Role,
                    model.IsActive
                );
                
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(UserManagement));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.DeleteUser)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(UserManagement));
        }
        
        [RequirePermission(Permission.ManageUsers)]
        public async Task<IActionResult> ManagePermissions(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }
            
            // Get all permissions with their status for this user
            var userPermissions = await _userService.GetAllPermissionsWithStatusAsync(user.Id);
            
            // Group permissions by category
            var groupedPermissions = new Dictionary<PermissionCategory, List<PermissionItem>>();
            
            foreach (var permission in userPermissions)
            {
                var category = GetPermissionCategory(permission.Key);
                var item = new PermissionItem
                {
                    Permission = permission.Key,
                    DisplayName = GetPermissionDisplayName(permission.Key),
                    Description = GetPermissionDescription(permission.Key),
                    IsGranted = permission.Value
                };
                
                if (!groupedPermissions.ContainsKey(category))
                {
                    groupedPermissions[category] = new List<PermissionItem>();
                }
                
                groupedPermissions[category].Add(item);
            }
            
            var viewModel = new UserPermissionsViewModel
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                GroupedPermissions = groupedPermissions
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageUsers)]
        public async Task<IActionResult> UpdatePermissions(int userId, List<string> grantedPermissions)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }
            
            try
            {
                // Convert string permissions to enum values
                var permissionEnums = grantedPermissions
                    .Select(p => Enum.Parse<Permission>(p))
                    .ToList();
                
                // Get current permissions
                var currentPermissions = await _userService.GetUserPermissionsAsync(userId);
                
                // Remove permissions that are not in the new list
                foreach (var permission in currentPermissions)
                {
                    if (!permissionEnums.Contains(permission))
                    {
                        await _userService.RemovePermissionFromUserAsync(userId, permission);
                    }
                }
                
                // Add new permissions
                foreach (var permission in permissionEnums)
                {
                    await _userService.AddPermissionToUserAsync(userId, permission);
                }
                
                TempData["SuccessMessage"] = "Permissions updated successfully.";
                return RedirectToAction(nameof(UserManagement));
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(ManagePermissions), new { id = userId });
            }
        }
        
        private PermissionCategory GetPermissionCategory(Permission permission)
        {
            return permission.ToString() switch
            {
                var p when p.Contains("User") => PermissionCategory.UserManagement,
                var p when p.Contains("Announcement") => PermissionCategory.AnnouncementManagement,
                var p when p.Contains("Bill") || p.Contains("Payment") => PermissionCategory.BillingManagement,
                var p when p.Contains("Facility") || p.Contains("Reservation") => PermissionCategory.FacilityManagement,
                var p when p.Contains("Service") => PermissionCategory.ServiceRequestManagement,
                var p when p.Contains("Document") => PermissionCategory.DocumentManagement,
                var p when p.Contains("Forum") => PermissionCategory.ForumManagement,
                var p when p.Contains("Security") || p.Contains("Visitor") || p.Contains("Vehicle") => PermissionCategory.SecurityManagement,
                var p when p.Contains("Report") => PermissionCategory.ReportsAndAnalytics,
                _ => PermissionCategory.UserManagement
            };
        }
        
        private string GetPermissionDisplayName(Permission permission)
        {
            return permission.ToString().SplitCamelCase();
        }
        
        private string GetPermissionDescription(Permission permission)
        {
            return permission switch
            {
                Permission.ManageUsers => "Access to view and manage all user accounts",
                Permission.CreateUser => "Ability to create new user accounts",
                Permission.EditUser => "Ability to edit existing user accounts",
                Permission.DeleteUser => "Ability to delete user accounts",
                
                Permission.ManageAnnouncements => "Access to view and manage all announcements",
                Permission.CreateAnnouncement => "Ability to create new announcements",
                Permission.EditAnnouncement => "Ability to edit existing announcements",
                Permission.DeleteAnnouncement => "Ability to delete announcements",
                
                Permission.ManageBilling => "Access to view and manage all billing information",
                Permission.GenerateBills => "Ability to generate bills for homeowners",
                Permission.ProcessPayments => "Ability to process and record payments",
                Permission.ViewPaymentReports => "Ability to view payment reports",
                
                Permission.ManageFacilities => "Access to view and manage all facilities",
                Permission.ApproveReservations => "Ability to approve or deny facility reservations",
                
                Permission.ManageServiceRequests => "Access to view and manage all service requests",
                Permission.AssignServiceRequests => "Ability to assign service requests to staff",
                Permission.ResolveServiceRequests => "Ability to mark service requests as resolved",
                
                Permission.ManageDocuments => "Access to view and manage all documents",
                Permission.UploadDocuments => "Ability to upload new documents",
                Permission.DeleteDocuments => "Ability to delete documents",
                
                Permission.ManageForum => "Access to view and manage the community forum",
                Permission.ModerateForumPosts => "Ability to edit or delete forum posts",
                
                Permission.ManageSecurity => "Access to view and manage security features",
                Permission.ApproveVisitorPasses => "Ability to approve visitor passes",
                Permission.ManageVehicleRegistration => "Ability to manage vehicle registrations",
                
                Permission.AccessReports => "Access to view system reports and analytics",
                Permission.ExportReports => "Ability to export reports in various formats",
                
                _ => "No description available"
            };
        }
        
        #endregion
        
        #region Announcements
        
        public IActionResult Announcements()
        {
            return View();
        }
        
        public IActionResult CreateAnnouncement()
        {
            return View(new AnnouncementCreateViewModel 
            {
                UrgencyLevel = TheQuel.Models.UrgencyLevel.Medium,
                NotificationMethod = TheQuel.Models.NotificationMethod.OnSite
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAnnouncement(AnnouncementCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // TODO: Implement announcement creation logic
            
            TempData["SuccessMessage"] = "Announcement created successfully.";
            return RedirectToAction(nameof(Announcements));
        }
        
        #endregion
        
        #region Billing
        
        public IActionResult BillingDashboard()
        {
            return View();
        }
        
        public IActionResult GenerateBills()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateBills(BillGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // TODO: Implement bill generation logic
            
            TempData["SuccessMessage"] = "Bills generated successfully.";
            return RedirectToAction(nameof(BillingDashboard));
        }
        
        public IActionResult PaymentReports()
        {
            return View();
        }
        
        #endregion
        
        #region Facilities
        
        public IActionResult FacilityReservations()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveReservation(int id)
        {
            // TODO: Implement reservation approval logic
            
            TempData["SuccessMessage"] = "Reservation approved.";
            return RedirectToAction(nameof(FacilityReservations));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclineReservation(int id, string reason)
        {
            // TODO: Implement reservation decline logic
            
            TempData["SuccessMessage"] = "Reservation declined.";
            return RedirectToAction(nameof(FacilityReservations));
        }
        
        #endregion
        
        #region Service Requests
        
        public IActionResult ServiceRequests()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignServiceRequest(int requestId, int staffId)
        {
            // TODO: Implement service request assignment logic
            
            TempData["SuccessMessage"] = "Service request assigned.";
            return RedirectToAction(nameof(ServiceRequests));
        }
        
        #endregion
        
        #region Documents
        
        public IActionResult Documents()
        {
            return View();
        }
        
        public IActionResult UploadDocument()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadDocument(DocumentUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // TODO: Implement document upload logic
            
            TempData["SuccessMessage"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Documents));
        }
        
        #endregion
        
        #region Reports
        
        public IActionResult Reports()
        {
            return View();
        }
        
        public IActionResult GenerateReport(string reportType)
        {
            // TODO: Implement report generation logic
            
            return View();
        }
        
        #endregion
    }
} 