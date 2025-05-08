using System;

namespace TheQuel.Core
{
    public class FacilityReservation
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int FacilityId { get; set; }
        public int UserId { get; set; }
        
        // Reservation details
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int? ExpectedAttendees { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Status tracking
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? AdminRemarks { get; set; }
        public int? ReviewedByUserId { get; set; }
        
        // Notification flags
        public bool NotificationSent { get; set; } = false;
        public DateTime? NotificationSentAt { get; set; }
        
        // Navigation properties
        public virtual Facility Facility { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User? ReviewedByUser { get; set; }
        
        // Helper methods for timespan calculation
        public TimeSpan GetDuration() => EndTime - StartTime;
        
        public bool OverlapsWith(FacilityReservation other)
        {
            if (FacilityId != other.FacilityId || ReservationDate != other.ReservationDate)
                return false;
                
            return (StartTime < other.EndTime && EndTime > other.StartTime);
        }
    }
    
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
} 