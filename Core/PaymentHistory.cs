using System;

namespace TheQuel.Core
{
    public class PaymentHistory
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public PaymentStatus? PreviousStatus { get; set; }
        public PaymentStatus? NewStatus { get; set; }
        public decimal? PreviousAmount { get; set; }
        public decimal? NewAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Payment Payment { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
} 