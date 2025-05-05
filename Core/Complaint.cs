using System;

namespace TheQuel.Core
{
    public class Complaint
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplaintStatus Status { get; set; }
        public ComplaintCategory Category { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public int? AssignedToId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User AssignedTo { get; set; } = null!;
    }
    
    public enum ComplaintStatus
    {
        New,
        InProgress,
        Resolved,
        Rejected
    }
    
    public enum ComplaintCategory
    {
        Noise,
        Security,
        Maintenance,
        Utilities,
        Neighbor,
        Other
    }
} 