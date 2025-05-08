using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class BillingDashboardViewModel
    {
        public decimal TotalYearlyPayments { get; set; }
        public decimal TotalMonthlyPayments { get; set; }
        public int PendingPaymentsCount { get; set; }
        public int OverduePaymentsCount { get; set; }
        public IEnumerable<Payment> RecentPayments { get; set; } = new List<Payment>();
        public IEnumerable<Payment> PendingBills { get; set; } = new List<Payment>();
    }
    
    public class PaymentReportsViewModel
    {
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public int StatusId { get; set; }
        public int TypeId { get; set; }
        public string ReportTitle { get; set; } = string.Empty;
        public List<Payment> ReportData { get; set; } = new List<Payment>();
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        
        public List<SelectListItem> ReportTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Years { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Months { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentStatuses { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
    }
}