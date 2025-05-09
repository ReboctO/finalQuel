using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IAnnouncementService _announcementService;
        private readonly IPaymentService _paymentService;
        private readonly IFacilityService _facilityService;
        private readonly IFacilityReservationService _facilityReservationService;
        
        public AdminController(
            IUserService userService, 
            IPropertyService propertyService, 
            IAnnouncementService announcementService,
            IPaymentService paymentService,
            IFacilityService facilityService,
            IFacilityReservationService facilityReservationService)
        {
            _userService = userService;
            _propertyService = propertyService;
            _announcementService = announcementService;
            _paymentService = paymentService;
            _facilityService = facilityService;
            _facilityReservationService = facilityReservationService;
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
        
        [RequirePermission(Permission.ManageAnnouncements)]
        public async Task<IActionResult> Announcements()
        {
            var model = new AnnouncementsViewModel
            {
                ActiveAnnouncements = await _announcementService.GetActiveAnnouncementsAsync(),
                ScheduledAnnouncements = await _announcementService.GetScheduledAnnouncementsAsync(),
                ArchivedAnnouncements = await _announcementService.GetArchivedAnnouncementsAsync()
            };
            
            return View(model);
        }
        
        [RequirePermission(Permission.CreateAnnouncement)]
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
        [RequirePermission(Permission.CreateAnnouncement)]
        public async Task<IActionResult> CreateAnnouncement(AnnouncementCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _announcementService.CreateAnnouncementAsync(model, userId);
                
                TempData["SuccessMessage"] = "Announcement created successfully.";
                return RedirectToAction(nameof(Announcements));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [RequirePermission(Permission.EditAnnouncement)]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            
            if (announcement == null)
            {
                return NotFound();
            }
            
            var model = new AnnouncementEditViewModel
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                UrgencyLevel = (TheQuel.Models.UrgencyLevel)announcement.UrgencyLevel,
                NotificationMethod = (TheQuel.Models.NotificationMethod)announcement.NotificationMethod,
                IsActive = announcement.IsActive,
                SendEmail = announcement.NotificationMethod == TheQuel.Core.NotificationMethod.Email || announcement.NotificationMethod == TheQuel.Core.NotificationMethod.All,
                SendSMS = announcement.NotificationMethod == TheQuel.Core.NotificationMethod.SMS || announcement.NotificationMethod == TheQuel.Core.NotificationMethod.All
            };
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.EditAnnouncement)]
        public async Task<IActionResult> EditAnnouncement(AnnouncementEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var announcement = await _announcementService.GetAnnouncementByIdAsync(model.Id);
                
                if (announcement == null)
                {
                    return NotFound();
                }
                
                announcement.Title = model.Title;
                announcement.Content = model.Content;
                announcement.UrgencyLevel = (TheQuel.Core.UrgencyLevel)model.UrgencyLevel;
                announcement.NotificationMethod = (TheQuel.Core.NotificationMethod)model.NotificationMethod;
                announcement.IsActive = model.IsActive;
                
                await _announcementService.UpdateAnnouncementAsync(announcement);
                
                TempData["SuccessMessage"] = "Announcement updated successfully.";
                return RedirectToAction(nameof(Announcements));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.DeleteAnnouncement)]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            try
            {
                await _announcementService.DeleteAnnouncementAsync(id);
                TempData["SuccessMessage"] = "Announcement deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Announcements));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageAnnouncements)]
        public async Task<IActionResult> ArchiveAnnouncement(int id)
        {
            try
            {
                await _announcementService.ArchiveAnnouncementAsync(id);
                TempData["SuccessMessage"] = "Announcement archived successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Announcements));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageAnnouncements)]
        public async Task<IActionResult> PublishAnnouncement(int id)
        {
            try
            {
                await _announcementService.PublishAnnouncementAsync(id);
                TempData["SuccessMessage"] = "Announcement published successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Announcements));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageAnnouncements)]
        public async Task<IActionResult> RestoreAnnouncement(int id)
        {
            try
            {
                await _announcementService.RestoreAnnouncementAsync(id);
                TempData["SuccessMessage"] = "Announcement restored successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Announcements));
        }
        
        #endregion
        
        #region Billing
        
        [RequirePermission(Permission.ManageBilling)]
        public async Task<IActionResult> BillingDashboard()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            
            var model = new BillingDashboardViewModel
            {
                TotalYearlyPayments = await _paymentService.GetTotalPaymentsForYearAsync(currentYear),
                TotalMonthlyPayments = await _paymentService.GetTotalPaymentsForMonthAsync(currentYear, currentMonth),
                PendingPaymentsCount = await _paymentService.GetPendingPaymentsCountAsync(),
                OverduePaymentsCount = await _paymentService.GetOverduePaymentsCountAsync(),
                
                // Get recent payments
                RecentPayments = (await _paymentService.GetAllPaymentsAsync())
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10)
                    .ToList(),
                
                // Get pending bills
                PendingBills = (await _paymentService.GetPaymentsByStatusAsync(PaymentStatus.Pending))
                    .OrderBy(p => p.DueDate)
                    .Take(10)
                    .ToList()
            };
            
            return View(model);
        }
        
        [RequirePermission(Permission.GenerateBills)]
        public IActionResult GenerateBills()
        {
            var model = new BillGenerationViewModel
            {
                DueDate = DateTime.Now.AddDays(30),
                GenerateForAllHomeowners = true
            };
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.GenerateBills)]
        public async Task<IActionResult> GenerateBills(BillGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var generatedBills = await _paymentService.GenerateBillsAsync(
                    model.BillType,
                    model.Amount,
                    model.DueDate,
                    model.Notes,
                    model.GenerateForAllHomeowners);
                
                TempData["SuccessMessage"] = $"Successfully generated {generatedBills.Count()} bills for homeowners.";
                return RedirectToAction(nameof(BillingDashboard));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        [RequirePermission(Permission.ViewPaymentReports)]
        public IActionResult PaymentReports()
        {
            var model = new PaymentReportsViewModel
            {
                CurrentYear = DateTime.Now.Year,
                CurrentMonth = DateTime.Now.Month,
                ReportTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "monthly", Text = "Monthly Payments" },
                    new SelectListItem { Value = "yearly", Text = "Yearly Payments" },
                    new SelectListItem { Value = "status", Text = "Payments by Status" },
                    new SelectListItem { Value = "type", Text = "Payments by Type" }
                },
                PaymentStatuses = Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.ToString()
                    })
                    .ToList(),
                PaymentTypes = Enum.GetValues(typeof(PaymentType))
                    .Cast<PaymentType>()
                    .Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.ToString()
                    })
                    .ToList(),
                Years = Enumerable.Range(DateTime.Now.Year - 5, 6)
                    .Select(y => new SelectListItem
                    {
                        Value = y.ToString(),
                        Text = y.ToString()
                    })
                    .ToList(),
                Months = Enumerable.Range(1, 12)
                    .Select(m => new SelectListItem
                    {
                        Value = m.ToString(),
                        Text = new DateTime(2000, m, 1).ToString("MMMM")
                    })
                    .ToList()
            };
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ViewPaymentReports)]
        public async Task<IActionResult> GeneratePaymentReport(PaymentReportsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("PaymentReports", model);
            }
            
            IEnumerable<Payment> reportData = new List<Payment>();
            string reportTitle = "";
            
            try
            {
                switch (model.ReportType)
                {
                    case "monthly":
                        var startDate = new DateTime(model.Year, model.Month, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);
                        reportData = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);
                        reportTitle = $"Monthly Payments Report - {startDate:MMMM yyyy}";
                        break;
                        
                    case "yearly":
                        var yearStart = new DateTime(model.Year, 1, 1);
                        var yearEnd = new DateTime(model.Year, 12, 31);
                        reportData = await _paymentService.GetPaymentsByDateRangeAsync(yearStart, yearEnd);
                        reportTitle = $"Yearly Payments Report - {model.Year}";
                        break;
                        
                    case "status":
                        reportData = await _paymentService.GetPaymentsByStatusAsync((PaymentStatus)model.StatusId);
                        reportTitle = $"Payments by Status - {(PaymentStatus)model.StatusId}";
                        break;
                        
                    case "type":
                        reportData = await _paymentService.GetAllPaymentsAsync();
                        reportData = reportData.Where(p => p.Type == (PaymentType)model.TypeId);
                        reportTitle = $"Payments by Type - {(PaymentType)model.TypeId}";
                        break;
                        
                    default:
                        ModelState.AddModelError("ReportType", "Invalid report type selected");
                        return View("PaymentReports", model);
                }
                
                model.ReportTitle = reportTitle;
                model.ReportData = reportData.ToList();
                model.TotalAmount = reportData.Sum(p => p.Amount);
                model.PaidAmount = reportData.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
                model.PendingAmount = reportData.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount);
                model.OverdueAmount = reportData.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount);
                
                return View("PaymentReportResults", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("PaymentReports", model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ProcessPayments)]
        public async Task<IActionResult> ProcessPayment(int paymentId, string referenceNumber)
        {
            try
            {
                await _paymentService.RecordPaymentAsync(paymentId, DateTime.Now, referenceNumber);
                TempData["SuccessMessage"] = "Payment processed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(BillingDashboard));
        }
        
        #endregion
        
        #region Facilities
        
        [RequirePermission(Permission.ManageFacilities)]
        public async Task<IActionResult> FacilityManagement()
        {
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return View(facilities);
        }
        
        [RequirePermission(Permission.ManageFacilities)]
        public IActionResult CreateFacility()
        {
            return View(new FacilityCreateViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageFacilities)]
        public async Task<IActionResult> CreateFacility(FacilityCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                await _facilityService.CreateFacilityAsync(
                    model.Name,
                    model.Description,
                    model.Type,
                    model.Capacity,
                    model.Location,
                    model.ImageUrl,
                    model.HourlyRate,
                    TimeSpan.Parse(model.OpeningTime),
                    TimeSpan.Parse(model.ClosingTime),
                    model.MaxDaysInAdvance,
                    model.MaxReservationsPerUser,
                    model.RequiresAdminApproval);
                TempData["SuccessMessage"] = "Facility created successfully.";
                return RedirectToAction(nameof(FacilityManagement));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [RequirePermission(Permission.ManageFacilities)]
        public async Task<IActionResult> EditFacility(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            
            var viewModel = new FacilityEditViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                Type = facility.Type,
                Capacity = facility.Capacity,
                IsActive = facility.IsActive,
                Location = facility.Location,
                ImageUrl = facility.ImageUrl,
                HourlyRate = facility.HourlyRate,
                OpeningTime = facility.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = facility.ClosingTime.ToString(@"hh\:mm"),
                MaxDaysInAdvance = facility.MaxDaysInAdvance,
                MaxReservationsPerUser = facility.MaxReservationsPerUser,
                RequiresAdminApproval = facility.RequiresAdminApproval
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageFacilities)]
        public async Task<IActionResult> EditFacility(FacilityEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                await _facilityService.UpdateFacilityAsync(
                    model.Id,
                    model.Name,
                    model.Description,
                    model.Type,
                    model.Capacity,
                    model.IsActive,
                    model.Location,
                    model.ImageUrl,
                    model.HourlyRate,
                    TimeSpan.Parse(model.OpeningTime),
                    TimeSpan.Parse(model.ClosingTime),
                    model.MaxDaysInAdvance,
                    model.MaxReservationsPerUser,
                    model.RequiresAdminApproval);
                TempData["SuccessMessage"] = "Facility updated successfully.";
                return RedirectToAction(nameof(FacilityManagement));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ManageFacilities)]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            try
            {
                await _facilityService.DeleteFacilityAsync(id);
                TempData["SuccessMessage"] = "Facility deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(FacilityManagement));
        }
        
        #endregion
        
        #region Facility Reservations
        
        [RequirePermission(Permission.ApproveReservations)]
        public async Task<IActionResult> FacilityReservations(ReservationStatus? status, string searchTerm, DateTime? startDate, DateTime? endDate, int? facilityId)
        {
            var reservations = await _facilityReservationService.GetAllReservationsAsync();
            
            // Apply filters
            if (status.HasValue)
            {
                reservations = reservations.Where(r => r.Status == status.Value);
            }
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                reservations = reservations.Where(r => 
                    r.User.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.User.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.User.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Facility.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Purpose.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            if (startDate.HasValue)
            {
                reservations = reservations.Where(r => r.ReservationDate >= startDate.Value.Date);
            }
            
            if (endDate.HasValue)
            {
                reservations = reservations.Where(r => r.ReservationDate <= endDate.Value.Date);
            }
            
            if (facilityId.HasValue)
            {
                reservations = reservations.Where(r => r.FacilityId == facilityId.Value);
            }
            
            // Get facilities for dropdown
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            var facilitySelectList = facilities.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            }).ToList();
            
            // Add "All Facilities" option
            facilitySelectList.Insert(0, new SelectListItem { Value = "", Text = "All Facilities" });
            
            var viewModel = new AdminReservationListViewModel
            {
                Reservations = reservations.Select(r => MapReservationToAdminViewModel(r)).ToList(),
                FilterStatus = status,
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate,
                FacilityId = facilityId,
                Facilities = facilitySelectList
            };
            
            return View(viewModel);
        }
        
        [RequirePermission(Permission.ApproveReservations)]
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var reservation = await _facilityReservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            var viewModel = MapReservationToAdminDetailsViewModel(reservation);
            return View(viewModel);
        }
        
        [RequirePermission(Permission.ApproveReservations)]
        public async Task<IActionResult> ReviewReservation(int id)
        {
            var reservation = await _facilityReservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Check if the reservation is already reviewed
            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = "This reservation has already been reviewed.";
                return RedirectToAction(nameof(ReservationDetails), new { id = reservation.Id });
            }
            
            var viewModel = new ReviewReservationViewModel
            {
                Id = reservation.Id,
                FacilityName = reservation.Facility?.Name ?? "Unknown",
                UserName = $"{reservation.User?.FirstName} {reservation.User?.LastName}",
                UserEmail = reservation.User?.Email ?? "Unknown",
                Date = reservation.ReservationDate.ToShortDateString(),
                StartTime = reservation.StartTime.ToString(@"hh\:mm"),
                EndTime = reservation.EndTime.ToString(@"hh\:mm"),
                Purpose = reservation.Purpose,
                ExpectedAttendees = reservation.ExpectedAttendees,
                Notes = reservation.Notes,
                Decision = "Approve"
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ApproveReservations)]
        public async Task<IActionResult> ReviewReservation(ReviewReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (model.Decision == "Approve")
                {
                    await _facilityReservationService.ApproveReservationAsync(model.Id, userId, model.Remarks);
                    TempData["SuccessMessage"] = "Reservation approved successfully.";
                }
                else
                {
                    await _facilityReservationService.RejectReservationAsync(model.Id, userId, model.Remarks);
                    TempData["SuccessMessage"] = "Reservation rejected successfully.";
                }
                
                return RedirectToAction(nameof(FacilityReservations));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permission.ApproveReservations)]
        public async Task<IActionResult> CancelReservation(int id, string remarks)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _facilityReservationService.AdminCancelReservationAsync(id, userId, remarks);
                TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(FacilityReservations));
        }
        
        private AdminReservationViewModel MapReservationToAdminViewModel(FacilityReservation reservation)
        {
            var now = DateTime.Now;
            var reservationDateTime = reservation.ReservationDate.Add(reservation.StartTime);
            var isPast = reservationDateTime < now;
            
            // Ensure User is loaded
            if (reservation.User == null)
            {
                // Try to load the user if not already loaded
                reservation.User = _userService.GetUserByIdAsync(reservation.UserId).GetAwaiter().GetResult();
            }
            
            string userName = "Unknown";
            string userEmail = "Unknown";
            
            if (reservation.User != null)
            {
                userName = string.IsNullOrWhiteSpace(reservation.User.FirstName) && string.IsNullOrWhiteSpace(reservation.User.LastName)
                    ? "User #" + reservation.UserId
                    : $"{reservation.User.FirstName} {reservation.User.LastName}".Trim();
                
                userEmail = string.IsNullOrWhiteSpace(reservation.User.Email)
                    ? "No email provided"
                    : reservation.User.Email;
            }
            
            return new AdminReservationViewModel
            {
                Id = reservation.Id,
                FacilityId = reservation.FacilityId,
                FacilityName = reservation.Facility?.Name ?? "Unknown",
                UserName = userName,
                UserEmail = userEmail,
                Date = reservation.ReservationDate.ToShortDateString(),
                StartTime = reservation.StartTime.ToString(@"hh\:mm"),
                EndTime = reservation.EndTime.ToString(@"hh\:mm"),
                Duration = (reservation.EndTime - reservation.StartTime).ToString(@"h\:mm"),
                Purpose = reservation.Purpose,
                ExpectedAttendees = reservation.ExpectedAttendees,
                Notes = reservation.Notes,
                Status = reservation.Status,
                AdminRemarks = reservation.AdminRemarks ?? "",
                ReviewedByUser = reservation.ReviewedByUser != null 
                    ? $"{reservation.ReviewedByUser.FirstName} {reservation.ReviewedByUser.LastName}" 
                    : "",
                CreatedAt = reservation.CreatedAt.ToString("g"),
                IsPast = isPast,
                CanReview = reservation.Status == ReservationStatus.Pending && !isPast,
                CanCancel = reservation.Status != ReservationStatus.Cancelled && !isPast
            };
        }
        
        private AdminReservationDetailsViewModel MapReservationToAdminDetailsViewModel(FacilityReservation reservation)
        {
            var viewModel = MapReservationToAdminViewModel(reservation);
            
            return new AdminReservationDetailsViewModel
            {
                Id = viewModel.Id,
                FacilityId = viewModel.FacilityId,
                FacilityName = viewModel.FacilityName,
                UserName = viewModel.UserName,
                UserEmail = viewModel.UserEmail,
                Date = viewModel.Date,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                Duration = viewModel.Duration,
                Purpose = viewModel.Purpose,
                ExpectedAttendees = viewModel.ExpectedAttendees,
                Notes = viewModel.Notes,
                Status = viewModel.Status,
                AdminRemarks = viewModel.AdminRemarks,
                ReviewedByUser = viewModel.ReviewedByUser,
                CreatedAt = viewModel.CreatedAt,
                UpdatedAt = reservation.UpdatedAt?.ToString("g") ?? "",
                ApprovedAt = reservation.ApprovedAt?.ToString("g") ?? "",
                RejectedAt = reservation.RejectedAt?.ToString("g") ?? "",
                IsPast = viewModel.IsPast,
                CanReview = viewModel.CanReview,
                CanCancel = viewModel.CanCancel
            };
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