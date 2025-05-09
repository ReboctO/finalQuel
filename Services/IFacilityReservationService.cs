using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IFacilityReservationService
    {
        Task<FacilityReservation> GetReservationByIdAsync(int id);
        Task<IEnumerable<FacilityReservation>> GetAllReservationsAsync();
        Task<IEnumerable<FacilityReservation>> GetReservationsByFacilityAsync(int facilityId);
        Task<IEnumerable<FacilityReservation>> GetReservationsByUserAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetUpcomingReservationsAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetPastReservationsAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetPendingReservationsAsync();
        Task<IEnumerable<FacilityReservation>> GetReservationsForDateAsync(DateTime date);
        Task<IEnumerable<FacilityReservation>> GetReservationsForDateRangeAsync(DateTime startDate, DateTime endDate);
        
        Task<FacilityReservation> CreateReservationAsync(
            int userId, 
            int facilityId, 
            DateTime reservationDate, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            string purpose, 
            int? expectedAttendees, 
            string notes);
            
        Task<FacilityReservation> UpdateReservationAsync(
            int id,
            DateTime reservationDate, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            string purpose, 
            int? expectedAttendees, 
            string notes);
            
        Task<FacilityReservation> ApproveReservationAsync(int id, int adminUserId, string? remarks = null);
        Task<FacilityReservation> RejectReservationAsync(int id, int adminUserId, string remarks);
        Task<FacilityReservation> CancelReservationAsync(int id);
        Task<bool> DeleteReservationAsync(int id);
        
        Task<bool> IsTimeSlotAvailableAsync(int facilityId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null);
        Task<bool> HasUserReachedWeeklyLimitAsync(int userId, int facilityId);
        Task SendReservationNotificationAsync(int reservationId, string notificationType);
        Task<Dictionary<DateTime, List<TimeSlot>>> GetAvailableTimeSlotsAsync(int facilityId, DateTime startDate, int daysToShow = 7);
    }
    
    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public int? ExistingReservationId { get; set; }
    }
} 