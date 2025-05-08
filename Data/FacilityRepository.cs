using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class FacilityRepository : Repository<Facility>, IFacilityRepository
    {
        public FacilityRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Facility>> GetActiveFacilitiesAsync()
        {
            return await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Facility>> GetFacilitiesByTypeAsync(FacilityType type)
        {
            return await _context.Facilities
                .Where(f => f.Type == type && f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }
        
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            return !await _context.Facilities.AnyAsync(f => f.Name == name);
        }
        
        public async Task<IEnumerable<Facility>> GetFacilitiesWithAvailabilityAsync(DateTime date)
        {
            // Get all active facilities
            var facilities = await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();
                
            // For each facility, get reservations for the specified date
            foreach (var facility in facilities)
            {
                facility.Reservations = await _context.FacilityReservations
                    .Where(r => r.FacilityId == facility.Id && 
                           r.ReservationDate.Date == date.Date && 
                           (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.Pending))
                    .ToListAsync();
            }
            
            return facilities;
        }
    }
} 