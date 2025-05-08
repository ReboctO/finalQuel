using System.Collections.Generic;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class StaffDashboardViewModel
    {
        public User User { get; set; } = null!;
        
        // Add staff dashboard specific properties
        public int AssignedTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public int PendingTasksCount { get; set; }
        
        public IEnumerable<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
    }
} 