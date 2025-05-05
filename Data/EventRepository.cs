using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _context.Events
                .Where(e => e.StartDate > DateTime.Now)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> GetEventsByTypeAsync(EventType type)
        {
            return await _context.Events
                .Where(e => e.Type == type)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Events
                .Where(e => (e.StartDate >= start && e.StartDate <= end) || 
                           (e.EndDate >= start && e.EndDate <= end))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> GetEventsByCreatorAsync(int createdById)
        {
            return await _context.Events
                .Where(e => e.CreatedById == createdById)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> GetEventsAttendedByUserAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.Attendees.Any(a => a.UserId == userId))
                .ToListAsync();
        }
    }
} 