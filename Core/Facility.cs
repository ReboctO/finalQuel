using System;
using System.Collections.Generic;

namespace TheQuel.Core
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FacilityType Type { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public string Location { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal? HourlyRate { get; set; }
        public TimeSpan? MinimumReservationTime { get; set; }
        public TimeSpan? MaximumReservationTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Opening hours (24-hour format)
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0);  // 8:00 AM
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0); // 10:00 PM
        
        // For special rules
        public int MaxDaysInAdvance { get; set; } = 30; // How many days in advance can be booked
        public int MaxReservationsPerUser { get; set; } = 2; // Per week
        public bool RequiresAdminApproval { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<FacilityReservation> Reservations { get; set; } = new List<FacilityReservation>();
    }
    
    public enum FacilityType
    {
        FunctionHall,
        BasketballCourt,
        SwimmingPool,
        Gym,
        TennisCourt,
        MeetingRoom,
        BBQArea,
        Other
    }
} 