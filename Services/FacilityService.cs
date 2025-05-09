using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly IFacilityReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public FacilityService(
            IFacilityRepository facilityRepository,
            IFacilityReservationRepository reservationRepository,
            IUnitOfWork unitOfWork)
        {
            _facilityRepository = facilityRepository;
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<IEnumerable<Facility>> GetAllFacilitiesAsync()
        {
            return await _facilityRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Facility>> GetActiveFacilitiesAsync()
        {
            return await _facilityRepository.GetActiveFacilitiesAsync();
        }
        
        public async Task<Facility> GetFacilityByIdAsync(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {id} not found.");
            }
            return facility;
        }
        
        public async Task<IEnumerable<Facility>> GetFacilitiesByTypeAsync(FacilityType type)
        {
            return await _facilityRepository.GetFacilitiesByTypeAsync(type);
        }
        
        public async Task<IEnumerable<Facility>> GetAvailableFacilitiesForDateAsync(DateTime date)
        {
            return await _facilityRepository.GetFacilitiesWithAvailabilityAsync(date);
        }
        
        public async Task<Facility> CreateFacilityAsync(
            string name, 
            string description, 
            FacilityType type, 
            int capacity, 
            string location, 
            string imageUrl, 
            decimal? hourlyRate, 
            TimeSpan openingTime, 
            TimeSpan closingTime, 
            int maxDaysInAdvance, 
            int maxReservationsPerUser, 
            bool requiresAdminApproval)
        {
            // Check if name is unique
            bool isNameUnique = await _facilityRepository.IsNameUniqueAsync(name);
            if (!isNameUnique)
            {
                throw new InvalidOperationException($"A facility with the name '{name}' already exists.");
            }
            
            // Basic validation
            if (openingTime >= closingTime)
            {
                throw new InvalidOperationException("Opening time must be before closing time.");
            }
            
            if (maxDaysInAdvance < 1)
            {
                throw new InvalidOperationException("Maximum days in advance must be at least 1.");
            }
            
            if (maxReservationsPerUser < 1)
            {
                throw new InvalidOperationException("Maximum reservations per user must be at least 1.");
            }
            
            // Create new facility
            var facility = new Facility
            {
                Name = name,
                Description = description,
                Type = type,
                Capacity = capacity,
                Location = location,
                ImageUrl = imageUrl,
                HourlyRate = hourlyRate,
                OpeningTime = openingTime,
                ClosingTime = closingTime,
                MaxDaysInAdvance = maxDaysInAdvance,
                MaxReservationsPerUser = maxReservationsPerUser,
                RequiresAdminApproval = requiresAdminApproval,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            
            await _facilityRepository.AddAsync(facility);
            await _unitOfWork.SaveChangesAsync();
            
            return facility;
        }
        
        public async Task<Facility> UpdateFacilityAsync(
            int id, 
            string name, 
            string description, 
            FacilityType type, 
            int capacity, 
            bool isActive, 
            string location, 
            string imageUrl, 
            decimal? hourlyRate, 
            TimeSpan openingTime, 
            TimeSpan closingTime, 
            int maxDaysInAdvance, 
            int maxReservationsPerUser, 
            bool requiresAdminApproval)
        {
            // Get facility
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {id} not found.");
            }
            
            // Check if name is unique if changed
            if (facility.Name != name)
            {
                bool isNameUnique = await _facilityRepository.IsNameUniqueAsync(name);
                if (!isNameUnique)
                {
                    throw new InvalidOperationException($"A facility with the name '{name}' already exists.");
                }
            }
            
            // Basic validation
            if (openingTime >= closingTime)
            {
                throw new InvalidOperationException("Opening time must be before closing time.");
            }
            
            if (maxDaysInAdvance < 1)
            {
                throw new InvalidOperationException("Maximum days in advance must be at least 1.");
            }
            
            if (maxReservationsPerUser < 1)
            {
                throw new InvalidOperationException("Maximum reservations per user must be at least 1.");
            }
            
            // Update facility
            facility.Name = name;
            facility.Description = description;
            facility.Type = type;
            facility.Capacity = capacity;
            facility.IsActive = isActive;
            facility.Location = location;
            facility.ImageUrl = imageUrl;
            facility.HourlyRate = hourlyRate;
            facility.OpeningTime = openingTime;
            facility.ClosingTime = closingTime;
            facility.MaxDaysInAdvance = maxDaysInAdvance;
            facility.MaxReservationsPerUser = maxReservationsPerUser;
            facility.RequiresAdminApproval = requiresAdminApproval;
            facility.UpdatedAt = DateTime.Now;
            
            await _facilityRepository.UpdateAsync(facility);
            await _unitOfWork.SaveChangesAsync();
            
            return facility;
        }
        
        public async Task<bool> DeleteFacilityAsync(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility == null)
            {
                return false;
            }
            
            await _facilityRepository.RemoveAsync(facility);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> IsFacilityAvailableAsync(
            int facilityId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            int? excludeReservationId = null)
        {
            // Get facility
            var facility = await _facilityRepository.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException($"Facility with ID {facilityId} not found.");
            }
            
            // Check if the facility is active
            if (!facility.IsActive)
            {
                return false;
            }
            
            // Check if the reservation date is within allowed range
            var maxDate = DateTime.Now.AddDays(facility.MaxDaysInAdvance);
            if (date.Date > maxDate.Date || date.Date < DateTime.Now.Date)
            {
                return false;
            }
            
            // Check if the time is within opening hours
            if (startTime < facility.OpeningTime || endTime > facility.ClosingTime)
            {
                return false;
            }
            
            // Check for overlapping reservations
            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(
                facilityId, date, startTime, endTime, excludeReservationId);
                
            return !overlappingReservations.Any();
        }
        
        public async Task<bool> CanUserMakeReservationAsync(int userId, DateTime reservationDate)
        {
            // Get the start of the week for the reservation date
            var startOfWeek = reservationDate.Date.AddDays(-(int)reservationDate.DayOfWeek);
            
            // Get the user's reservation count for the week
            var weeklyCount = await _reservationRepository.GetUserReservationCountForWeekAsync(userId, startOfWeek);
            
            // Default max reservations per week (can be made configurable)
            int maxReservationsPerWeek = 5;
            
            return weeklyCount < maxReservationsPerWeek;
        }
    }
} 