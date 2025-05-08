using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IFacilityReservationRepository : IRepository<FacilityReservation>
    {
        Task<IEnumerable<FacilityReservation>> GetReservationsByFacilityAsync(int facilityId);
        Task<IEnumerable<FacilityReservation>> GetReservationsByUserAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetReservationsByStatusAsync(ReservationStatus status);
        Task<IEnumerable<FacilityReservation>> GetReservationsForDateAsync(DateTime date);
        Task<IEnumerable<FacilityReservation>> GetReservationsForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<FacilityReservation>> GetUpcomingReservationsAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetPastReservationsAsync(int userId);
        Task<IEnumerable<FacilityReservation>> GetOverlappingReservationsAsync(int facilityId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null);
        Task<int> GetUserReservationCountForWeekAsync(int userId, DateTime weekStartDate);
    }
} 