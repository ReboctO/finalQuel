using System;
using System.Collections.Generic;

namespace TheQuel.Core
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public EventType Type { get; set; }
        public bool IsPublic { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
    }
    
    public class EventAttendee
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public AttendanceStatus Status { get; set; }
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
    
    public enum EventType
    {
        Meeting,
        SocialGathering,
        Maintenance,
        Emergency,
        Holiday,
        Other
    }
    
    public enum AttendanceStatus
    {
        Going,
        NotGoing,
        Maybe
    }
} 