using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IFacilityService
    {
        Task<IEnumerable<Facility>> GetAllFacilitiesAsync();
        Task<IEnumerable<Facility>> GetActiveFacilitiesAsync();
        Task<Facility> GetFacilityByIdAsync(int id);
        Task<IEnumerable<Facility>> GetFacilitiesByTypeAsync(FacilityType type);
        Task<IEnumerable<Facility>> GetAvailableFacilitiesForDateAsync(DateTime date);
        Task<Facility> CreateFacilityAsync(string name, string description, FacilityType type, int capacity, 
                                          string location, string imageUrl, decimal? hourlyRate, 
                                          TimeSpan openingTime, TimeSpan closingTime, 
                                          int maxDaysInAdvance, int maxReservationsPerUser, 
                                          bool requiresAdminApproval);
        Task<Facility> UpdateFacilityAsync(int id, string name, string description, FacilityType type, 
                                          int capacity, bool isActive, string location, string imageUrl, 
                                          decimal? hourlyRate, TimeSpan openingTime, TimeSpan closingTime, 
                                          int maxDaysInAdvance, int maxReservationsPerUser, 
                                          bool requiresAdminApproval);
        Task<bool> DeleteFacilityAsync(int id);
        Task<bool> IsFacilityAvailableAsync(int facilityId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null);
        Task<bool> CanUserMakeReservationAsync(int userId, DateTime reservationDate);
    }
} 