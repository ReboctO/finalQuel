using System;
using System.Collections.Generic;

namespace TheQuel.Core
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual Property? Property { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
        public virtual ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
    
    public enum UserRole
    {
        Admin,
        HomeOwner,
        Staff
    }
    
    public enum Permission
    {
        // User Management
        ManageUsers,
        CreateUser,
        EditUser,
        DeleteUser,
        
        // Announcement Management
        ManageAnnouncements,
        CreateAnnouncement,
        EditAnnouncement,
        DeleteAnnouncement,
        
        // Billing Management
        ManageBilling,
        GenerateBills,
        ProcessPayments,
        ViewPaymentReports,
        
        // Facility Management
        ManageFacilities,
        ApproveReservations,
        
        // Service Request Management
        ManageServiceRequests,
        AssignServiceRequests,
        ResolveServiceRequests,
        
        // Document Management
        ManageDocuments,
        UploadDocuments,
        DeleteDocuments,
        
        // Forum Management
        ManageForum,
        ModerateForumPosts,
        
        // Security Management
        ManageSecurity,
        ApproveVisitorPasses,
        ManageVehicleRegistration,
        
        // Reports and Analytics
        AccessReports,
        ExportReports
    }
    
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Permission Permission { get; set; }
        public virtual User User { get; set; } = null!;
    }
} 