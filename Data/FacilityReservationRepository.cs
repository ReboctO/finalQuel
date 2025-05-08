using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class FacilityReservationRepository : Repository<FacilityReservation>, IFacilityReservationRepository
    {
        public FacilityReservationRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsByFacilityAsync(int facilityId)
        {
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Where(r => r.FacilityId == facilityId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsByUserAsync(int userId)
        {
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsByStatusAsync(ReservationStatus status)
        {
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsForDateAsync(DateTime date)
        {
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Where(r => r.ReservationDate.Date == date.Date)
                .OrderBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetReservationsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Where(r => r.ReservationDate.Date >= startDate.Date && r.ReservationDate.Date <= endDate.Date)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetUpcomingReservationsAsync(int userId)
        {
            var today = DateTime.Today;
            
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId && 
                       (r.ReservationDate.Date > today || 
                        (r.ReservationDate.Date == today && r.StartTime > DateTime.Now.TimeOfDay)))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetPastReservationsAsync(int userId)
        {
            var today = DateTime.Today;
            
            return await _context.FacilityReservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId && 
                       (r.ReservationDate.Date < today || 
                        (r.ReservationDate.Date == today && r.EndTime <= DateTime.Now.TimeOfDay)))
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.EndTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<FacilityReservation>> GetOverlappingReservationsAsync(
            int facilityId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            int? excludeReservationId = null)
        {
            var query = _context.FacilityReservations
                .Where(r => r.FacilityId == facilityId &&
                       r.ReservationDate.Date == date.Date &&
                       r.Status != ReservationStatus.Rejected &&
                       r.Status != ReservationStatus.Cancelled &&
                       startTime < r.EndTime &&
                       endTime > r.StartTime);
                       
            if (excludeReservationId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }
            
            return await query.ToListAsync();
        }
        
        public async Task<int> GetUserReservationCountForWeekAsync(int userId, DateTime weekStartDate)
        {
            var weekEndDate = weekStartDate.AddDays(7);
            
            return await _context.FacilityReservations
                .CountAsync(r => r.UserId == userId &&
                           r.ReservationDate >= weekStartDate &&
                           r.ReservationDate < weekEndDate &&
                           r.Status != ReservationStatus.Rejected &&
                           r.Status != ReservationStatus.Cancelled);
        }
    }
} 