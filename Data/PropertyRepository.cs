using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public PropertyRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Property>> GetAvailablePropertiesAsync()
        {
            return await _context.Properties
                .Where(p => p.Status == PropertyStatus.Available)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Property>> GetPropertiesByOwnerAsync(int ownerId)
        {
            return await _context.Properties
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status)
        {
            return await _context.Properties
                .Where(p => p.Status == status)
                .ToListAsync();
        }
    }
} 