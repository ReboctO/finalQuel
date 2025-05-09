using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IFacilityRepository : IRepository<Facility>
    {
        Task<IEnumerable<Facility>> GetActiveFacilitiesAsync();
        Task<IEnumerable<Facility>> GetFacilitiesByTypeAsync(FacilityType type);
        Task<bool> IsNameUniqueAsync(string name);
        Task<IEnumerable<Facility>> GetFacilitiesWithAvailabilityAsync(DateTime date);
    }
} 