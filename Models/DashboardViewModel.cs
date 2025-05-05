using System.Collections.Generic;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class DashboardViewModel
    {
        public User User { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public IEnumerable<Payment> RecentPayments { get; set; } = new List<Payment>();
        public IEnumerable<Complaint> RecentComplaints { get; set; } = new List<Complaint>();
        public IEnumerable<Event> UpcomingEvents { get; set; } = new List<Event>();
    }
} 