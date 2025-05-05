using System;

namespace TheQuel.Core
{
    public class Payment
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public PaymentType Type { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Property Property { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
    
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Overdue,
        Cancelled
    }
    
    public enum PaymentType
    {
        MaintenanceFee,
        SecurityFee,
        UtilityFee,
        PropertyTax,
        SpecialAssessment,
        Other
    }
} 