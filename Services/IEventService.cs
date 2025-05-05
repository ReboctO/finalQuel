using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IEventService
    {
        Task<Event> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetEventsByTypeAsync(EventType type);
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Event>> GetEventsByCreatorAsync(int createdById);
        Task<IEnumerable<Event>> GetEventsAttendedByUserAsync(int userId);
        Task<Event> CreateEventAsync(Event @event);
        Task<Event> UpdateEventAsync(Event @event);
        Task<bool> DeleteEventAsync(int id);
        Task<EventAttendee> AddAttendeeAsync(int eventId, int userId, AttendanceStatus status);
        Task<EventAttendee> UpdateAttendeeStatusAsync(int eventId, int userId, AttendanceStatus status);
    }
} 