using System;
using System.Collections.Generic;

namespace TheQuel.Core
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public UrgencyLevel UrgencyLevel { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public NotificationMethod NotificationMethod { get; set; }
        public bool EmailSent { get; set; }
        public bool SmsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<AnnouncementRecipient> Recipients { get; set; } = new List<AnnouncementRecipient>();
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
    
    public class AnnouncementRecipient
    {
        public int Id { get; set; }
        public int AnnouncementId { get; set; }
        public int UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public virtual Announcement Announcement { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
} 