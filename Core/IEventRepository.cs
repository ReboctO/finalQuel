using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetEventsByTypeAsync(EventType type);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Event>> GetEventsByCreatorAsync(int createdById);
        Task<IEnumerable<Event>> GetEventsAttendedByUserAsync(int userId);
    }
} 