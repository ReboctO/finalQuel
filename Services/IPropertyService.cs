using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IPropertyService
    {
        Task<Property?> GetPropertyByIdAsync(int id);
        Task<IEnumerable<Property>> GetAllPropertiesAsync();
        Task<IEnumerable<Property>> GetAvailablePropertiesAsync();
        Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status);
        Task<IEnumerable<Property>> GetPropertiesByOwnerAsync(int ownerId);
        Task<Property> CreatePropertyAsync(Property property);
        Task<Property> UpdatePropertyAsync(Property property);
        Task<bool> DeletePropertyAsync(int id);
        Task<bool> AssignOwnerAsync(int propertyId, int ownerId);
    }
} 