using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "HomeOwner")]
    public class FacilityController : Controller
    {
        private readonly IFacilityService _facilityService;
        private readonly IFacilityReservationService _reservationService;
        
        public FacilityController(
            IFacilityService facilityService,
            IFacilityReservationService reservationService)
        {
            _facilityService = facilityService;
            _reservationService = reservationService;
        }
        
        // GET: /Facility
        public async Task<IActionResult> Index(string searchTerm, FacilityType? type, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            
            // Get facilities
            var facilities = type.HasValue
                ? await _facilityService.GetFacilitiesByTypeAsync(type.Value)
                : await _facilityService.GetActiveFacilitiesAsync();
                
            // Filter by search term if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                facilities = facilities.Where(f => 
                    f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    f.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            // Map to view model
            var viewModel = new FacilityListViewModel
            {
                Facilities = facilities.Select(f => new FacilityViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Type = f.Type,
                    Capacity = f.Capacity,
                    IsActive = f.IsActive,
                    Location = f.Location,
                    ImageUrl = f.ImageUrl,
                    HourlyRate = f.HourlyRate,
                    OpeningTime = f.OpeningTime.ToString(@"hh\:mm"),
                    ClosingTime = f.ClosingTime.ToString(@"hh\:mm"),
                    MaxDaysInAdvance = f.MaxDaysInAdvance,
                    MaxReservationsPerUser = f.MaxReservationsPerUser,
                    RequiresAdminApproval = f.RequiresAdminApproval,
                    // Check if any reservations for today
                    IsAvailableToday = !f.Reservations.Any(r => 
                        r.ReservationDate.Date == DateTime.Today.Date &&
                        (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.Pending))
                }).ToList(),
                SelectedDate = selectedDate,
                SearchTerm = searchTerm,
                FilterType = type
            };
            
            return View(viewModel);
        }
        
        // GET: /Facility/Details/5
        public async Task<IActionResult> Details(int id, DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            
            // Get available time slots
            var timeSlots = await _reservationService.GetAvailableTimeSlotsAsync(id, selectedDate, 7);
            
            // Create facility view model
            var viewModel = new FacilityViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                Type = facility.Type,
                Capacity = facility.Capacity,
                IsActive = facility.IsActive,
                Location = facility.Location,
                ImageUrl = facility.ImageUrl,
                HourlyRate = facility.HourlyRate,
                OpeningTime = facility.OpeningTime.ToString(@"hh\:mm"),
                ClosingTime = facility.ClosingTime.ToString(@"hh\:mm"),
                MaxDaysInAdvance = facility.MaxDaysInAdvance,
                MaxReservationsPerUser = facility.MaxReservationsPerUser,
                RequiresAdminApproval = facility.RequiresAdminApproval
            };
            
            // Add time slots for the selected date
            if (timeSlots.TryGetValue(selectedDate, out var slots))
            {
                viewModel.AvailableTimeSlots = slots.Select(ts => new TimeSlotViewModel
                {
                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
                    IsAvailable = ts.IsAvailable,
                    ExistingReservationId = ts.ExistingReservationId
                }).ToList();
            }
            
            return View(viewModel);
        }
        
        // GET: /Facility/Reserve/5
        public async Task<IActionResult> Reserve(int id, DateTime? date)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            
            var selectedDate = date ?? DateTime.Today;
            
            // Get available time slots
            var timeSlots = await _reservationService.GetAvailableTimeSlotsAsync(id, selectedDate, 7);
            
            // Create reservation view model
            var viewModel = new CreateReservationViewModel
            {
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                ReservationDate = selectedDate
            };
            
            // Add time slots for the selected date
            if (timeSlots.TryGetValue(selectedDate, out var slots))
            {
                viewModel.AvailableTimeSlots = slots.Select(ts => new TimeSlotViewModel
                {
                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
                    IsAvailable = ts.IsAvailable,
                    ExistingReservationId = ts.ExistingReservationId
                }).ToList();
            }
            
            return View(viewModel);
        }
        
        // POST: /Facility/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(CreateReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload time slots
                var timeSlots = await _reservationService.GetAvailableTimeSlotsAsync(
                    model.FacilityId, model.ReservationDate, 7);
                    
                if (timeSlots.TryGetValue(model.ReservationDate, out var slots))
                {
                    model.AvailableTimeSlots = slots.Select(ts => new TimeSlotViewModel
                    {
                        StartTime = ts.StartTime.ToString(@"hh\:mm"),
                        EndTime = ts.EndTime.ToString(@"hh\:mm"),
                        IsAvailable = ts.IsAvailable,
                        ExistingReservationId = ts.ExistingReservationId
                    }).ToList();
                }
                
                return View(model);
            }
            
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    ModelState.AddModelError("", "Unable to identify current user");
                    return View(model);
                }
                
                int userId = int.Parse(userIdClaim);
                
                // Parse time values
                if (!TimeSpan.TryParse(model.StartTime, out var startTime) ||
                    !TimeSpan.TryParse(model.EndTime, out var endTime))
                {
                    ModelState.AddModelError("", "Invalid time format");
                    return View(model);
                }
                
                // Create reservation
                await _reservationService.CreateReservationAsync(
                    userId,
                    model.FacilityId,
                    model.ReservationDate,
                    startTime,
                    endTime,
                    model.Purpose,
                    model.ExpectedAttendees,
                    model.Notes
                );
                
                TempData["SuccessMessage"] = "Your reservation has been submitted and is pending approval.";
                return RedirectToAction(nameof(MyReservations));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                
                // Reload time slots
                var timeSlots = await _reservationService.GetAvailableTimeSlotsAsync(
                    model.FacilityId, model.ReservationDate, 7);
                    
                if (timeSlots.TryGetValue(model.ReservationDate, out var slots))
                {
                    model.AvailableTimeSlots = slots.Select(ts => new TimeSlotViewModel
                    {
                        StartTime = ts.StartTime.ToString(@"hh\:mm"),
                        EndTime = ts.EndTime.ToString(@"hh\:mm"),
                        IsAvailable = ts.IsAvailable,
                        ExistingReservationId = ts.ExistingReservationId
                    }).ToList();
                }
                
                return View(model);
            }
        }
        
        // GET: /Facility/MyReservations
        public async Task<IActionResult> MyReservations(ReservationStatus? status, DateTime? startDate, DateTime? endDate)
        {
            // Get current user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            
            int userId = int.Parse(userIdClaim);
            
            // Get user reservations
            var userReservations = await _reservationService.GetReservationsByUserAsync(userId);
            
            // Apply filters
            if (status.HasValue)
            {
                userReservations = userReservations.Where(r => r.Status == status.Value);
            }
            
            if (startDate.HasValue)
            {
                userReservations = userReservations.Where(r => r.ReservationDate >= startDate.Value.Date);
            }
            
            if (endDate.HasValue)
            {
                userReservations = userReservations.Where(r => r.ReservationDate <= endDate.Value.Date);
            }
            
            // Map to view model
            var viewModel = new FacilityReservationListViewModel
            {
                Reservations = userReservations.Select(r => MapReservationToViewModel(r)).ToList(),
                FilterStatus = status,
                StartDate = startDate,
                EndDate = endDate
            };
            
            return View(viewModel);
        }
        
        // GET: /Facility/ReservationDetails/5
        public async Task<IActionResult> ReservationDetails(int id)
        {
            // Get current user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            
            int userId = int.Parse(userIdClaim);
            
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Check if the reservation belongs to the current user
            if (reservation.UserId != userId)
            {
                return Forbid();
            }
            
            // Map to view model
            var viewModel = MapReservationToDetailsViewModel(reservation);
            
            return View(viewModel);
        }
        
        // GET: /Facility/CancelReservation/5
        public async Task<IActionResult> CancelReservation(int id)
        {
            // Get current user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            
            int userId = int.Parse(userIdClaim);
            
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Check if the reservation belongs to the current user
            if (reservation.UserId != userId)
            {
                return Forbid();
            }
            
            // Map to view model
            var viewModel = MapReservationToViewModel(reservation);
            
            return View(viewModel);
        }
        
        // POST: /Facility/CancelReservation/5
        [HttpPost, ActionName("CancelReservation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservationConfirmed(int id)
        {
            // Get current user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            
            int userId = int.Parse(userIdClaim);
            
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Check if the reservation belongs to the current user
            if (reservation.UserId != userId)
            {
                return Forbid();
            }
            
            try
            {
                await _reservationService.CancelReservationAsync(id);
                TempData["SuccessMessage"] = "Your reservation has been cancelled successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(MyReservations));
        }
        
        private FacilityReservationViewModel MapReservationToViewModel(FacilityReservation reservation)
        {
            var now = DateTime.Now;
            var reservationDateTime = reservation.ReservationDate.Add(reservation.StartTime);
            var canCancel = reservation.Status != ReservationStatus.Cancelled && 
                            reservation.Status != ReservationStatus.Rejected &&
                            reservationDateTime > now.AddHours(24);
                            
            var canEdit = reservation.Status == ReservationStatus.Pending &&
                         reservationDateTime > now.AddHours(24);
            
            return new FacilityReservationViewModel
            {
                Id = reservation.Id,
                FacilityId = reservation.FacilityId,
                FacilityName = reservation.Facility?.Name ?? "Unknown",
                UserName = $"{reservation.User?.FirstName} {reservation.User?.LastName}",
                UserEmail = reservation.User?.Email ?? "Unknown",
                Date = reservation.ReservationDate.ToShortDateString(),
                StartTime = reservation.StartTime.ToString(@"hh\:mm"),
                EndTime = reservation.EndTime.ToString(@"hh\:mm"),
                Duration = (reservation.EndTime - reservation.StartTime).ToString(@"h\:mm"),
                Purpose = reservation.Purpose,
                ExpectedAttendees = reservation.ExpectedAttendees,
                Notes = reservation.Notes,
                Status = reservation.Status,
                AdminRemarks = reservation.AdminRemarks ?? "",
                ReviewedByUser = reservation.ReviewedByUser != null 
                    ? $"{reservation.ReviewedByUser.FirstName} {reservation.ReviewedByUser.LastName}" 
                    : "",
                CanCancel = canCancel,
                CanEdit = canEdit
            };
        }
        
        private ReservationDetailsViewModel MapReservationToDetailsViewModel(FacilityReservation reservation)
        {
            var viewModel = MapReservationToViewModel(reservation);
            
            return new ReservationDetailsViewModel
            {
                Id = viewModel.Id,
                FacilityId = viewModel.FacilityId,
                FacilityName = viewModel.FacilityName,
                UserName = viewModel.UserName,
                UserEmail = viewModel.UserEmail,
                Date = viewModel.Date,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                Duration = viewModel.Duration,
                Purpose = viewModel.Purpose,
                ExpectedAttendees = viewModel.ExpectedAttendees,
                Notes = viewModel.Notes,
                Status = viewModel.Status,
                AdminRemarks = viewModel.AdminRemarks,
                ReviewedByUser = viewModel.ReviewedByUser,
                CanCancel = viewModel.CanCancel,
                CanEdit = viewModel.CanEdit,
                CreatedAt = reservation.CreatedAt.ToString("g"),
                UpdatedAt = reservation.UpdatedAt?.ToString("g") ?? "",
                ApprovedAt = reservation.ApprovedAt?.ToString("g") ?? "",
                RejectedAt = reservation.RejectedAt?.ToString("g") ?? ""
            };
        }
    }
} 