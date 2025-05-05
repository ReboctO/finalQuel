using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IPropertyRepository : IRepository<Property>
    {
        Task<IEnumerable<Property>> GetAvailablePropertiesAsync();
        Task<IEnumerable<Property>> GetPropertiesByOwnerAsync(int ownerId);
        Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status);
    }
} 