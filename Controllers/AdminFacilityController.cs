using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;
using TheQuel.Services;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/facilities")]
    public class AdminFacilityController : Controller
    {
        private readonly IFacilityService _facilityService;
        private readonly IFacilityReservationService _reservationService;
        
        public AdminFacilityController(
            IFacilityService facilityService,
            IFacilityReservationService reservationService)
        {
            _facilityService = facilityService;
            _reservationService = reservationService;
        }
        
        // GET: /admin/facilities
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm, FacilityType? type)
        {
            // Get all facilities (including inactive)
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            
            // Filter by type if provided
            if (type.HasValue)
            {
                facilities = facilities.Where(f => f.Type == type.Value);
            }
            
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
                    RequiresAdminApproval = f.RequiresAdminApproval
                }).ToList(),
                SearchTerm = searchTerm,
                FilterType = type
            };
            
            return View(viewModel);
        }
        
        // GET: /admin/facilities/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new FacilityCreateViewModel());
        }
        
        // POST: /admin/facilities/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilityCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                // Parse time values
                if (!TimeSpan.TryParse(model.OpeningTime, out var openingTime) ||
                    !TimeSpan.TryParse(model.ClosingTime, out var closingTime))
                {
                    ModelState.AddModelError("", "Invalid time format");
                    return View(model);
                }
                
                // Create facility
                await _facilityService.CreateFacilityAsync(
                    model.Name,
                    model.Description,
                    model.Type,
                    model.Capacity,
                    model.Location,
                    model.ImageUrl,
                    model.HourlyRate,
                    openingTime,
                    closingTime,
                    model.MaxDaysInAdvance,
                    model.MaxReservationsPerUser,
                    model.RequiresAdminApproval
                );
                
                TempData["SuccessMessage"] = "Facility created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        
        // GET: /admin/facilities/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            
            var viewModel = new FacilityEditViewModel
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
            
            return View(viewModel);
        }
        
        // POST: /admin/facilities/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilityEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                // Parse time values
                if (!TimeSpan.TryParse(model.OpeningTime, out var openingTime) ||
                    !TimeSpan.TryParse(model.ClosingTime, out var closingTime))
                {
                    ModelState.AddModelError("", "Invalid time format");
                    return View(model);
                }
                
                // Update facility
                await _facilityService.UpdateFacilityAsync(
                    id,
                    model.Name,
                    model.Description,
                    model.Type,
                    model.Capacity,
                    model.IsActive,
                    model.Location,
                    model.ImageUrl,
                    model.HourlyRate,
                    openingTime,
                    closingTime,
                    model.MaxDaysInAdvance,
                    model.MaxReservationsPerUser,
                    model.RequiresAdminApproval
                );
                
                TempData["SuccessMessage"] = "Facility updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        
        // GET: /admin/facilities/delete/5
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var facility = await _facilityService.GetFacilityByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            
            return View(new FacilityViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                Type = facility.Type,
                Capacity = facility.Capacity,
                IsActive = facility.IsActive,
                Location = facility.Location
            });
        }
        
        // POST: /admin/facilities/delete/5
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _facilityService.DeleteFacilityAsync(id);
            TempData["SuccessMessage"] = "Facility deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        // GET: /admin/facilities/reservations
        [HttpGet("reservations")]
        public async Task<IActionResult> Reservations(ReservationStatus? status, int? facilityId, string userEmail, DateTime? startDate, DateTime? endDate)
        {
            // Get all reservations
            var reservations = await _reservationService.GetAllReservationsAsync();
            
            // Apply filters
            if (status.HasValue)
            {
                reservations = reservations.Where(r => r.Status == status.Value);
            }
            
            if (facilityId.HasValue)
            {
                reservations = reservations.Where(r => r.FacilityId == facilityId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                reservations = reservations.Where(r => 
                    r.User != null && 
                    r.User.Email.Contains(userEmail, StringComparison.OrdinalIgnoreCase));
            }
            
            if (startDate.HasValue)
            {
                reservations = reservations.Where(r => r.ReservationDate >= startDate.Value.Date);
            }
            
            if (endDate.HasValue)
            {
                reservations = reservations.Where(r => r.ReservationDate <= endDate.Value.Date);
            }
            
            // Map to view model
            var viewModel = new FacilityReservationListViewModel
            {
                Reservations = reservations.Select(r => new FacilityReservationViewModel
                {
                    Id = r.Id,
                    FacilityId = r.FacilityId,
                    FacilityName = r.Facility?.Name ?? "Unknown",
                    UserName = $"{r.User?.FirstName} {r.User?.LastName}",
                    UserEmail = r.User?.Email ?? "Unknown",
                    Date = r.ReservationDate.ToShortDateString(),
                    StartTime = r.StartTime.ToString(@"hh\:mm"),
                    EndTime = r.EndTime.ToString(@"hh\:mm"),
                    Duration = (r.EndTime - r.StartTime).ToString(@"h\:mm"),
                    Purpose = r.Purpose,
                    ExpectedAttendees = r.ExpectedAttendees,
                    Status = r.Status,
                    AdminRemarks = r.AdminRemarks ?? "",
                    ReviewedByUser = r.ReviewedByUser != null 
                        ? $"{r.ReviewedByUser.FirstName} {r.ReviewedByUser.LastName}" 
                        : ""
                }).ToList(),
                FilterStatus = status,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = userEmail
            };
            
            return View(viewModel);
        }
        
        // GET: /admin/facilities/reservation-details/5
        [HttpGet("reservation-details/{id}")]
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            var viewModel = new ReservationDetailsViewModel
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
                CreatedAt = reservation.CreatedAt.ToString("g"),
                UpdatedAt = reservation.UpdatedAt?.ToString("g") ?? "",
                ApprovedAt = reservation.ApprovedAt?.ToString("g") ?? "",
                RejectedAt = reservation.RejectedAt?.ToString("g") ?? ""
            };
            
            return View(viewModel);
        }
        
        // GET: /admin/facilities/review-reservation/5
        [HttpGet("review-reservation/{id}")]
        public async Task<IActionResult> ReviewReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            // Check if the reservation is pending
            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["ErrorMessage"] = $"Cannot review a reservation with status '{reservation.Status}'.";
                return RedirectToAction(nameof(ReservationDetails), new { id });
            }
            
            var viewModel = new ReviewReservationViewModel
            {
                Id = reservation.Id,
                FacilityName = reservation.Facility?.Name ?? "Unknown",
                UserName = $"{reservation.User?.FirstName} {reservation.User?.LastName}",
                UserEmail = reservation.User?.Email ?? "Unknown",
                Date = reservation.ReservationDate.ToShortDateString(),
                StartTime = reservation.StartTime.ToString(@"hh\:mm"),
                EndTime = reservation.EndTime.ToString(@"hh\:mm"),
                Purpose = reservation.Purpose,
                ExpectedAttendees = reservation.ExpectedAttendees,
                Notes = reservation.Notes,
                Decision = "Approve"
            };
            
            return View(viewModel);
        }
        
        // POST: /admin/facilities/review-reservation/5
        [HttpPost("review-reservation/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewReservation(int id, ReviewReservationViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                // Get current admin ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    ModelState.AddModelError("", "Unable to identify current admin user");
                    return View(model);
                }
                
                int adminUserId = int.Parse(userIdClaim);
                
                if (model.Decision == "Approve")
                {
                    await _reservationService.ApproveReservationAsync(id, adminUserId, model.Remarks);
                    TempData["SuccessMessage"] = "Reservation approved successfully.";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(model.Remarks))
                    {
                        ModelState.AddModelError("Remarks", "Please provide a reason for rejection.");
                        return View(model);
                    }
                    
                    await _reservationService.RejectReservationAsync(id, adminUserId, model.Remarks);
                    TempData["SuccessMessage"] = "Reservation rejected successfully.";
                }
                
                return RedirectToAction(nameof(Reservations));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        
        // GET: /admin/facilities/pending-reservations
        [HttpGet("pending-reservations")]
        public async Task<IActionResult> PendingReservations()
        {
            // Get pending reservations
            var pendingReservations = await _reservationService.GetPendingReservationsAsync();
            
            // Map to view model
            var viewModel = new FacilityReservationListViewModel
            {
                Reservations = pendingReservations.Select(r => new FacilityReservationViewModel
                {
                    Id = r.Id,
                    FacilityId = r.FacilityId,
                    FacilityName = r.Facility?.Name ?? "Unknown",
                    UserName = $"{r.User?.FirstName} {r.User?.LastName}",
                    UserEmail = r.User?.Email ?? "Unknown",
                    Date = r.ReservationDate.ToShortDateString(),
                    StartTime = r.StartTime.ToString(@"hh\:mm"),
                    EndTime = r.EndTime.ToString(@"hh\:mm"),
                    Duration = (r.EndTime - r.StartTime).ToString(@"h\:mm"),
                    Purpose = r.Purpose,
                    ExpectedAttendees = r.ExpectedAttendees,
                    Status = r.Status
                }).ToList()
            };
            
            return View(viewModel);
        }
        
        // GET: /admin/facilities/reports
        [HttpGet("reports")]
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, int? facilityId)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;
            
            // Get reservations for the specified date range
            var reservations = await _reservationService.GetReservationsForDateRangeAsync(start, end);
            
            // Filter by facility if provided
            if (facilityId.HasValue)
            {
                reservations = reservations.Where(r => r.FacilityId == facilityId.Value);
            }
            
            // Map to view model
            var viewModel = new FacilityReservationListViewModel
            {
                Reservations = reservations.Select(r => new FacilityReservationViewModel
                {
                    Id = r.Id,
                    FacilityId = r.FacilityId,
                    FacilityName = r.Facility?.Name ?? "Unknown",
                    UserName = $"{r.User?.FirstName} {r.User?.LastName}",
                    UserEmail = r.User?.Email ?? "Unknown",
                    Date = r.ReservationDate.ToShortDateString(),
                    StartTime = r.StartTime.ToString(@"hh\:mm"),
                    EndTime = r.EndTime.ToString(@"hh\:mm"),
                    Duration = (r.EndTime - r.StartTime).ToString(@"h\:mm"),
                    Purpose = r.Purpose,
                    ExpectedAttendees = r.ExpectedAttendees,
                    Status = r.Status,
                    AdminRemarks = r.AdminRemarks ?? "",
                    ReviewedByUser = r.ReviewedByUser != null 
                        ? $"{r.ReviewedByUser.FirstName} {r.ReviewedByUser.LastName}" 
                        : ""
                }).ToList(),
                StartDate = start,
                EndDate = end
            };
            
            return View(viewModel);
        }
    }
} 