using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHomeowners { get; set; }
        public int TotalStaff { get; set; }
        public int TotalProperties { get; set; }
        public int AvailableProperties { get; set; }
        public int PendingPayments { get; set; }
        public int PendingServiceRequests { get; set; }
        public int UpcomingEvents { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }
    }

    public class UserEditViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
    
    public class UserPermissionsViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public Dictionary<PermissionCategory, List<PermissionItem>> GroupedPermissions { get; set; } = new();
    }
    
    public class PermissionItem
    {
        public Permission Permission { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }
    
    public enum PermissionCategory
    {
        UserManagement,
        AnnouncementManagement,
        BillingManagement,
        FacilityManagement,
        ServiceRequestManagement,
        DocumentManagement,
        ForumManagement,
        SecurityManagement,
        ReportsAndAnalytics
    }

    public class AnnouncementCreateViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Urgency Level")]
        public UrgencyLevel UrgencyLevel { get; set; }
        
        [Required]
        [Display(Name = "Notification Method")]
        public NotificationMethod NotificationMethod { get; set; }
        
        [Display(Name = "Send Email")]
        public bool SendEmail { get; set; }
        
        [Display(Name = "Send SMS")]
        public bool SendSMS { get; set; }
    }
    
    public enum UrgencyLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum NotificationMethod
    {
        OnSite,
        Email,
        SMS,
        All
    }

    public class BillGenerationViewModel
    {
        [Required]
        [Display(Name = "Bill Type")]
        public PaymentType BillType { get; set; }
        
        [Required]
        [Display(Name = "Amount")]
        [Range(1, 100000)]
        public decimal Amount { get; set; }
        
        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Generate for All Homeowners")]
        public bool GenerateForAllHomeowners { get; set; }
    }

    public class DocumentUploadViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Category")]
        public DocumentCategory Category { get; set; }
        
        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;
        
        [Required]
        [Display(Name = "Visibility")]
        public DocumentVisibility Visibility { get; set; }
    }
    
    public enum DocumentCategory
    {
        Guidelines,
        Forms,
        Reports,
        Contracts,
        Other
    }
    
    public enum DocumentVisibility
    {
        AllUsers,
        AdminOnly,
        StaffOnly,
        HomeownersOnly
    }
} 