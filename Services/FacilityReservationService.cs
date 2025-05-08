using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public class FacilityReservationService : IFacilityReservationService
    {
        private readonly IFacilityReservationRepository _reservationRepository;
        private readonly IFacilityRepository _facilityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public FacilityReservationService(
            IFacilityReservationRepository reservationRepository,
            IFacilityRepository facilityRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _reservationRepository = reservationRepository;
            _facilityRepository = facilityRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<FacilityReservation> GetReservationByIdAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {id} not found.");
            }
            return reservation;
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetAllReservationsAsync()
        {
            return await _reservationRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsByFacilityAsync(int facilityId)
        {
            return await _reservationRepository.GetReservationsByFacilityAsync(facilityId);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsByUserAsync(int userId)
        {
            return await _reservationRepository.GetReservationsByUserAsync(userId);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetUpcomingReservationsAsync(int userId)
        {
            return await _reservationRepository.GetUpcomingReservationsAsync(userId);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetPastReservationsAsync(int userId)
        {
            return await _reservationRepository.GetPastReservationsAsync(userId);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetPendingReservationsAsync()
        {
            return await _reservationRepository.GetReservationsByStatusAsync(ReservationStatus.Pending);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsForDateAsync(DateTime date)
        {
            return await _reservationRepository.GetReservationsForDateAsync(date);
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _reservationRepository.GetReservationsForDateRangeAsync(startDate, endDate);
        }
        
        public async Task<FacilityReservation> CreateReservationAsync(
            int userId, 
            int facilityId, 
            DateTime reservationDate, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            string purpose, 
            int? expectedAttendees, 
            string notes)
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }
            
            // Check if facility exists
            var facility = await _facilityRepository.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {facilityId} not found.");
            }
            
            // Check if the facility is active
            if (!facility.IsActive)
            {
                throw new InvalidOperationException($"Facility '{facility.Name}' is not currently available for reservations.");
            }
            
            // Validate the reservation time
            if (startTime >= endTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }
            
            // Check if the date is valid (not in the past)
            if (reservationDate.Date < DateTime.Now.Date)
            {
                throw new InvalidOperationException("Cannot make reservations for past dates.");
            }
            
            // Check if the date is within the allowed range
            var maxDate = DateTime.Now.AddDays(facility.MaxDaysInAdvance);
            if (reservationDate.Date > maxDate.Date)
            {
                throw new InvalidOperationException($"Reservations can only be made up to {facility.MaxDaysInAdvance} days in advance.");
            }
            
            // Check if the time is within facility hours
            if (startTime < facility.OpeningTime || endTime > facility.ClosingTime)
            {
                throw new InvalidOperationException($"Reservation time must be within facility hours ({facility.OpeningTime} - {facility.ClosingTime}).");
            }
            
            // Check if the time slot is available
            bool isAvailable = await IsTimeSlotAvailableAsync(facilityId, reservationDate, startTime, endTime);
            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }
            
            // Check if user has reached their weekly limit
            bool canReserve = await HasUserReachedWeeklyLimitAsync(userId, facilityId);
            if (!canReserve)
            {
                throw new InvalidOperationException($"You have reached your weekly reservation limit for this facility ({facility.MaxReservationsPerUser} reservations per week).");
            }
            
            // Create the reservation
            var reservation = new FacilityReservation
            {
                UserId = userId,
                FacilityId = facilityId,
                ReservationDate = reservationDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                Purpose = purpose,
                ExpectedAttendees = expectedAttendees,
                Notes = notes,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };
            
            await _reservationRepository.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification if needed
            await SendReservationNotificationAsync(reservation.Id, "created");
            
            return reservation;
        }
        
        public async Task<FacilityReservation> UpdateReservationAsync(
            int id,
            DateTime reservationDate, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            string purpose, 
            int? expectedAttendees, 
            string notes)
        {
            // Get reservation
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {id} not found.");
            }
            
            // Check if the reservation can be updated (not approved/rejected yet)
            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot update a reservation with status '{reservation.Status}'.");
            }
            
            // Get facility
            var facility = await _facilityRepository.GetByIdAsync(reservation.FacilityId);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {reservation.FacilityId} not found.");
            }
            
            // Validate the reservation time
            if (startTime >= endTime)
            {
                throw new InvalidOperationException("Start time must be before end time.");
            }
            
            // Check if the date is valid (not in the past)
            if (reservationDate.Date < DateTime.Now.Date)
            {
                throw new InvalidOperationException("Cannot make reservations for past dates.");
            }
            
            // Check if the date is within the allowed range
            var maxDate = DateTime.Now.AddDays(facility.MaxDaysInAdvance);
            if (reservationDate.Date > maxDate.Date)
            {
                throw new InvalidOperationException($"Reservations can only be made up to {facility.MaxDaysInAdvance} days in advance.");
            }
            
            // Check if the time is within facility hours
            if (startTime < facility.OpeningTime || endTime > facility.ClosingTime)
            {
                throw new InvalidOperationException($"Reservation time must be within facility hours ({facility.OpeningTime} - {facility.ClosingTime}).");
            }
            
            // Check if the time slot is available (excluding this reservation)
            bool isAvailable = await IsTimeSlotAvailableAsync(reservation.FacilityId, reservationDate, startTime, endTime, id);
            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }
            
            // Update the reservation
            reservation.ReservationDate = reservationDate.Date;
            reservation.StartTime = startTime;
            reservation.EndTime = endTime;
            reservation.Purpose = purpose;
            reservation.ExpectedAttendees = expectedAttendees;
            reservation.Notes = notes;
            reservation.UpdatedAt = DateTime.Now;
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification if needed
            await SendReservationNotificationAsync(reservation.Id, "updated");
            
            return reservation;
        }
        
        public async Task<FacilityReservation> ApproveReservationAsync(int id, int adminUserId, string? remarks = null)
        {
            // Get reservation
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {id} not found.");
            }
            
            // Check if the reservation is pending
            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot approve a reservation with status '{reservation.Status}'.");
            }
            
            // Update the reservation
            reservation.Status = ReservationStatus.Approved;
            reservation.AdminRemarks = remarks;
            reservation.ReviewedByUserId = adminUserId;
            reservation.ApprovedAt = DateTime.Now;
            reservation.UpdatedAt = DateTime.Now;
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification
            await SendReservationNotificationAsync(reservation.Id, "approved");
            
            return reservation;
        }
        
        public async Task<FacilityReservation> RejectReservationAsync(int id, int adminUserId, string remarks)
        {
            // Get reservation
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {id} not found.");
            }
            
            // Check if the reservation is pending
            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot reject a reservation with status '{reservation.Status}'.");
            }
            
            // Update the reservation
            reservation.Status = ReservationStatus.Rejected;
            reservation.AdminRemarks = remarks;
            reservation.ReviewedByUserId = adminUserId;
            reservation.RejectedAt = DateTime.Now;
            reservation.UpdatedAt = DateTime.Now;
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification
            await SendReservationNotificationAsync(reservation.Id, "rejected");
            
            return reservation;
        }
        
        public async Task<FacilityReservation> CancelReservationAsync(int id)
        {
            // Get reservation
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {id} not found.");
            }
            
            // Check if the reservation can be cancelled (not already cancelled or rejected)
            if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Rejected)
            {
                throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");
            }
            
            // Check if the reservation is in the future
            var reservationDateTime = reservation.ReservationDate.Add(reservation.StartTime);
            if (reservationDateTime <= DateTime.Now.AddHours(24))
            {
                throw new InvalidOperationException("Reservations must be cancelled at least 24 hours in advance.");
            }
            
            // Update the reservation
            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.Now;
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Send notification
            await SendReservationNotificationAsync(reservation.Id, "cancelled");
            
            return reservation;
        }
        
        public async Task<bool> DeleteReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null)
            {
                return false;
            }
            
            await _reservationRepository.RemoveAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> IsTimeSlotAvailableAsync(
            int facilityId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            int? excludeReservationId = null)
        {
            // Check for overlapping reservations
            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(
                facilityId, date, startTime, endTime, excludeReservationId);
                
            return !overlappingReservations.Any();
        }
        
        public async Task<bool> HasUserReachedWeeklyLimitAsync(int userId, int facilityId)
        {
            // Get facility
            var facility = await _facilityRepository.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {facilityId} not found.");
            }
            
            // Get the start of the current week
            var currentDate = DateTime.Now;
            var startOfWeek = currentDate.Date.AddDays(-(int)currentDate.DayOfWeek);
            
            // Get the user's reservation count for the week
            var weeklyCount = await _reservationRepository.GetUserReservationCountForWeekAsync(userId, startOfWeek);
            
            return weeklyCount < facility.MaxReservationsPerUser;
        }
        
        public async Task SendReservationNotificationAsync(int reservationId, string notificationType)
        {
            // Get reservation
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation == null)
            {
                throw new InvalidOperationException($"Reservation with ID {reservationId} not found.");
            }
            
            // In a real implementation, this would send an email or push notification
            // For now, just mark the notification as sent
            reservation.NotificationSent = true;
            reservation.NotificationSentAt = DateTime.Now;
            
            await _reservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveChangesAsync();
            
            // Log the notification
            Console.WriteLine($"Notification sent for reservation {reservationId}. Type: {notificationType}");
        }
        
        public async Task<Dictionary<DateTime, List<TimeSlot>>> GetAvailableTimeSlotsAsync(
            int facilityId, 
            DateTime startDate, 
            int daysToShow = 7)
        {
            // Get facility
            var facility = await _facilityRepository.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {facilityId} not found.");
            }
            
            var result = new Dictionary<DateTime, List<TimeSlot>>();
            
            // Create time slots for each day
            for (int i = 0; i < daysToShow; i++)
            {
                var currentDate = startDate.AddDays(i);
                var timeSlots = new List<TimeSlot>();
                
                // Generate time slots in 1-hour increments
                for (int hour = facility.OpeningTime.Hours; hour < facility.ClosingTime.Hours; hour++)
                {
                    var slotStart = new TimeSpan(hour, 0, 0);
                    var slotEnd = new TimeSpan(hour + 1, 0, 0);
                    
                    // Check if the slot is available
                    var existingReservations = await _reservationRepository.GetOverlappingReservationsAsync(
                        facilityId, currentDate, slotStart, slotEnd);
                        
                    int? existingReservationId = null;
                    if (existingReservations.Any())
                    {
                        existingReservationId = existingReservations.First().Id;
                    }
                    
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = slotStart,
                        EndTime = slotEnd,
                        IsAvailable = !existingReservations.Any(),
                        ExistingReservationId = existingReservationId
                    });
                }
                
                result.Add(currentDate, timeSlots);
            }
            
            return result;
        }
    }
} 