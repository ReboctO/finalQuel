using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using TheQuel.Core;

namespace TheQuel.Models
{
    public class AdminReservationListViewModel
    {
        public List<AdminReservationViewModel> Reservations { get; set; } = new List<AdminReservationViewModel>();
        public string? SearchTerm { get; set; }
        public ReservationStatus? FilterStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FacilityId { get; set; }
        public List<SelectListItem> Facilities { get; set; } = new List<SelectListItem>();
    }
    
    public class AdminReservationViewModel
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
        public string CreatedAt { get; set; } = string.Empty;
        public bool IsPast { get; set; }
        public bool CanReview { get; set; }
        public bool CanCancel { get; set; }
    }
    
    public class AdminReservationDetailsViewModel : AdminReservationViewModel
    {
        public string UpdatedAt { get; set; } = string.Empty;
        public string ApprovedAt { get; set; } = string.Empty;
        public string RejectedAt { get; set; } = string.Empty;
    }
    
    public class FacilityReservationCalendarViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        public int? FacilityId { get; set; }
        public List<SelectListItem> Facilities { get; set; } = new List<SelectListItem>();
        public List<CalendarReservationViewModel> Reservations { get; set; } = new List<CalendarReservationViewModel>();
    }
    
    public class CalendarReservationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Color { get; set; } = string.Empty;
        public string TextColor { get; set; } = "#ffffff";
        public bool AllDay { get; set; } = false;
        public string Url { get; set; } = string.Empty;
    }
    
    public class ReservationReportViewModel
    {
        public DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DateTime EndDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
        public int? FacilityId { get; set; }
        public List<SelectListItem> Facilities { get; set; } = new List<SelectListItem>();
        public List<AdminReservationViewModel> Reservations { get; set; } = new List<AdminReservationViewModel>();
        public int TotalReservations { get; set; }
        public int ApprovedReservations { get; set; }
        public int RejectedReservations { get; set; }
        public int CancelledReservations { get; set; }
        public int PendingReservations { get; set; }
        public Dictionary<string, int> ReservationsByFacility { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ReservationsByDay { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ReservationsByUser { get; set; } = new Dictionary<string, int>();
    }
} 