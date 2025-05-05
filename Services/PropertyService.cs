using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserRepository _userRepository;
        
        public PropertyService(IPropertyRepository propertyRepository, IUserRepository userRepository)
        {
            _propertyRepository = propertyRepository;
            _userRepository = userRepository;
        }
        
        public async Task<Property?> GetPropertyByIdAsync(int id)
        {
            return await _propertyRepository.GetByIdAsync(id);
        }
        
        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _propertyRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Property>> GetAvailablePropertiesAsync()
        {
            return await _propertyRepository.GetAvailablePropertiesAsync();
        }
        
        public async Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status)
        {
            return await _propertyRepository.GetPropertiesByStatusAsync(status);
        }
        
        public async Task<IEnumerable<Property>> GetPropertiesByOwnerAsync(int ownerId)
        {
            return await _propertyRepository.GetPropertiesByOwnerAsync(ownerId);
        }
        
        public async Task<Property> CreatePropertyAsync(Property property)
        {
            property.CreatedAt = DateTime.Now;
            
            if (property.Status == PropertyStatus.Sold && property.OwnerId.HasValue)
            {
                var owner = await _userRepository.GetByIdAsync(property.OwnerId.Value);
                if (owner == null)
                {
                    throw new InvalidOperationException($"User with ID {property.OwnerId.Value} not found");
                }
            }
            else if (property.Status == PropertyStatus.Sold && !property.OwnerId.HasValue)
            {
                throw new InvalidOperationException("Property marked as Sold must have an owner assigned");
            }
            
            await _propertyRepository.AddAsync(property);
            return property;
        }
        
        public async Task<Property> UpdatePropertyAsync(Property property)
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(property.Id);
            
            if (existingProperty == null)
            {
                throw new InvalidOperationException($"Property with ID {property.Id} not found");
            }
            
            // Update property properties
            existingProperty.LotNumber = property.LotNumber;
            existingProperty.BlockNumber = property.BlockNumber;
            existingProperty.Address = property.Address;
            existingProperty.SquareMeters = property.SquareMeters;
            existingProperty.Status = property.Status;
            existingProperty.PurchaseDate = property.PurchaseDate;
            existingProperty.PurchasePrice = property.PurchasePrice;
            existingProperty.UpdatedAt = DateTime.Now;
            
            // If status is changed to Sold, require owner
            if (property.Status == PropertyStatus.Sold && !property.OwnerId.HasValue)
            {
                throw new InvalidOperationException("Property marked as Sold must have an owner assigned");
            }
            
            // If owner is changed, validate owner exists
            if (property.OwnerId.HasValue && existingProperty.OwnerId != property.OwnerId)
            {
                var owner = await _userRepository.GetByIdAsync(property.OwnerId.Value);
                if (owner == null)
                {
                    throw new InvalidOperationException($"User with ID {property.OwnerId.Value} not found");
                }
                
                existingProperty.OwnerId = property.OwnerId;
            }
            
            await _propertyRepository.UpdateAsync(existingProperty);
            return existingProperty;
        }
        
        public async Task<bool> DeletePropertyAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            
            if (property == null)
            {
                return false;
            }
            
            await _propertyRepository.RemoveAsync(property);
            return true;
        }
        
        public async Task<bool> AssignOwnerAsync(int propertyId, int ownerId)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            
            if (property == null)
            {
                throw new InvalidOperationException($"Property with ID {propertyId} not found");
            }
            
            var owner = await _userRepository.GetByIdAsync(ownerId);
            
            if (owner == null)
            {
                throw new InvalidOperationException($"User with ID {ownerId} not found");
            }
            
            property.OwnerId = ownerId;
            property.Status = PropertyStatus.Sold;
            property.UpdatedAt = DateTime.Now;
            
            await _propertyRepository.UpdateAsync(property);
            return true;
        }
    }
} 