using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class FacilityListViewModel
    {
        public List<FacilityViewModel> Facilities { get; set; } = new List<FacilityViewModel>();
        public DateTime? SelectedDate { get; set; }
        public string? SearchTerm { get; set; }
        public FacilityType? FilterType { get; set; }
    }
    
    public class FacilityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FacilityType Type { get; set; }
        public string TypeName => Type.ToString();
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public string Location { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal? HourlyRate { get; set; }
        public string OpeningTime { get; set; } = "08:00";
        public string ClosingTime { get; set; } = "22:00";
        public int MaxDaysInAdvance { get; set; } = 30;
        public int MaxReservationsPerUser { get; set; } = 2;
        public bool RequiresAdminApproval { get; set; } = true;
        public bool IsAvailableToday { get; set; }
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new List<TimeSlotViewModel>();
    }
    
    public class TimeSlotViewModel
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int? ExistingReservationId { get; set; }
    }
    
    public class FacilityCreateViewModel
    {
        [Required]
        [Display(Name = "Facility Name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Facility Type")]
        public FacilityType Type { get; set; }
        
        [Required]
        [Display(Name = "Capacity")]
        [Range(1, 1000)]
        public int Capacity { get; set; }
        
        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
        
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }
        
        [Required]
        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public string OpeningTime { get; set; } = "08:00";
        
        [Required]
        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public string ClosingTime { get; set; } = "22:00";
        
        [Required]
        [Display(Name = "Maximum Days in Advance")]
        [Range(1, 365)]
        public int MaxDaysInAdvance { get; set; } = 30;
        
        [Required]
        [Display(Name = "Maximum Reservations Per User")]
        [Range(1, 10)]
        public int MaxReservationsPerUser { get; set; } = 2;
        
        [Display(Name = "Requires Admin Approval")]
        public bool RequiresAdminApproval { get; set; } = true;
    }
    
    public class FacilityEditViewModel : FacilityCreateViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
    
    public class FacilityReservationListViewModel
    {
        public List<FacilityReservationViewModel> Reservations { get; set; } = new List<FacilityReservationViewModel>();
        public string? SearchTerm { get; set; }
        public ReservationStatus? FilterStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    
    public class FacilityReservationViewModel
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int? ExpectedAttendees { get; set; }
        public string Notes { get; set; } = string.Empty;
        public ReservationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string AdminRemarks { get; set; } = string.Empty;
        public string ReviewedByUser { get; set; } = string.Empty;
        public bool CanCancel { get; set; }
        public bool CanEdit { get; set; }
    }
    
    public class CreateReservationViewModel
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Today;
        
        [Required]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; } = "09:00";
        
        [Required]
        [Display(Name = "End Time")]
        public string EndTime { get; set; } = "10:00";
        
        [Required]
        [Display(Name = "Purpose")]
        [StringLength(100)]
        public string Purpose { get; set; } = string.Empty;
        
        [Display(Name = "Expected Attendees")]
        [Range(1, 1000)]
        public int? ExpectedAttendees { get; set; }
        
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; } = string.Empty;
        
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new List<TimeSlotViewModel>();
    }
    
    public class EditReservationViewModel : CreateReservationViewModel
    {
        public int Id { get; set; }
        public ReservationStatus Status { get; set; }
    }
    
    public class ReservationDetailsViewModel : FacilityReservationViewModel
    {
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string ApprovedAt { get; set; } = string.Empty;
        public string RejectedAt { get; set; } = string.Empty;
    }
    
    public class ReviewReservationViewModel
    {
        public int Id { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int? ExpectedAttendees { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Decision")]
        [Required]
        public string Decision { get; set; } = "Approve";  // "Approve" or "Reject"
        
        [Display(Name = "Admin Remarks")]
        public string Remarks { get; set; } = string.Empty;
    }
} 