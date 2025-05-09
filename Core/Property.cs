using System;
using System.Collections.Generic;

namespace TheQuel.Core
{
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public string BlockNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal SquareMeters { get; set; }
        public PropertyStatus Status { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int? OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User? Owner { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    
    public enum PropertyStatus
    {
        Available,
        Sold,
        Reserved,
        UnderMaintenance
    }
} 