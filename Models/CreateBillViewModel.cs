using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TheQuel.Models
{
    public class CreateBillViewModel
    {
        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required]
        [Display(Name = "Payment Type")]
        public int PaymentTypeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;

        // Dropdown options
        public List<SelectListItem> Properties { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
    }
} 