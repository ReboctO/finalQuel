using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;

namespace TheQuel.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IUserRepository _userRepository;
        
        public AnnouncementService(IAnnouncementRepository announcementRepository, IUserRepository userRepository)
        {
            _announcementRepository = announcementRepository;
            _userRepository = userRepository;
        }
        
        public async Task<Announcement> GetAnnouncementByIdAsync(int id)
        {
            return await _announcementRepository.GetByIdAsync(id) ?? throw new InvalidOperationException($"Announcement with ID {id} not found");
        }
        
        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _announcementRepository.GetAllAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
        {
            return await _announcementRepository.GetActiveAnnouncementsAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetArchivedAnnouncementsAsync()
        {
            return await _announcementRepository.GetArchivedAnnouncementsAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetScheduledAnnouncementsAsync()
        {
            return await _announcementRepository.GetScheduledAnnouncementsAsync();
        }
        
        public async Task<Announcement> CreateAnnouncementAsync(AnnouncementCreateViewModel model, int createdById)
        {
            var announcement = new Announcement
            {
                Title = model.Title,
                Content = model.Content,
                UrgencyLevel = (TheQuel.Core.UrgencyLevel)model.UrgencyLevel,
                NotificationMethod = (TheQuel.Core.NotificationMethod)model.NotificationMethod,
                PublishedDate = DateTime.Now,
                IsActive = true,
                EmailSent = model.SendEmail,
                SmsSent = model.SendSMS,
                CreatedAt = DateTime.Now,
                CreatedById = createdById
            };
            
            await _announcementRepository.AddAsync(announcement);
            
            // Create announcement recipients based on target audience
            // For now, we'll add all active users as recipients
            var users = await _userRepository.GetAllAsync();
            var recipients = users
                .Where(u => u.IsActive)
                .Select(u => new AnnouncementRecipient
                {
                    AnnouncementId = announcement.Id,
                    UserId = u.Id,
                    IsRead = false
                }).ToList();
                
            await _announcementRepository.AddAnnouncementRecipientsAsync(recipients);
            
            // If email or SMS notifications are enabled, send them
            if (model.SendEmail || model.SendSMS)
            {
                await SendAnnouncementNotificationsAsync(announcement.Id);
            }
            
            return announcement;
        }
        
        public async Task<Announcement> UpdateAnnouncementAsync(Announcement announcement)
        {
            announcement.UpdatedAt = DateTime.Now;
            await _announcementRepository.UpdateAsync(announcement);
            return announcement;
        }
        
        public async Task<bool> DeleteAnnouncementAsync(int id)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            
            if (announcement == null)
                return false;
                
            await _announcementRepository.RemoveAsync(announcement);
            return true;
        }
        
        public async Task<bool> ArchiveAnnouncementAsync(int id)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            
            if (announcement == null)
                return false;
                
            announcement.IsActive = false;
            announcement.UpdatedAt = DateTime.Now;
            
            await _announcementRepository.UpdateAsync(announcement);
            return true;
        }
        
        public async Task<bool> PublishAnnouncementAsync(int id)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            
            if (announcement == null)
                return false;
                
            announcement.IsActive = true;
            announcement.PublishedDate = DateTime.Now;
            announcement.UpdatedAt = DateTime.Now;
            
            await _announcementRepository.UpdateAsync(announcement);
            return true;
        }
        
        public async Task<bool> RestoreAnnouncementAsync(int id)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id);
            
            if (announcement == null)
                return false;
                
            announcement.IsActive = true;
            announcement.UpdatedAt = DateTime.Now;
            
            await _announcementRepository.UpdateAsync(announcement);
            return true;
        }
        
        public async Task<bool> MarkAnnouncementAsReadAsync(int announcementId, int userId)
        {
            return await _announcementRepository.MarkAnnouncementAsReadAsync(announcementId, userId);
        }
        
        public async Task<IEnumerable<Announcement>> GetAnnouncementsForUserAsync(int userId)
        {
            return await _announcementRepository.GetAnnouncementsForUserAsync(userId);
        }
        
        public async Task<int> SendAnnouncementNotificationsAsync(int announcementId)
        {
            var announcement = await _announcementRepository.GetByIdAsync(announcementId);
            
            if (announcement == null)
                return 0;
                
            var recipients = await _announcementRepository.GetAnnouncementRecipientsAsync(announcementId);
            
            int notificationsSent = 0;
            
            // In a real-world application, you would integrate with email and SMS services here
            // For now, just mark the announcements as sent
            
            if (announcement.NotificationMethod == TheQuel.Core.NotificationMethod.Email || announcement.NotificationMethod == TheQuel.Core.NotificationMethod.All)
            {
                announcement.EmailSent = true;
                notificationsSent += recipients.Count();
            }
            
            if (announcement.NotificationMethod == TheQuel.Core.NotificationMethod.SMS || announcement.NotificationMethod == TheQuel.Core.NotificationMethod.All)
            {
                announcement.SmsSent = true;
                notificationsSent += recipients.Count();
            }
            
            announcement.UpdatedAt = DateTime.Now;
            await _announcementRepository.UpdateAsync(announcement);
            
            return notificationsSent;
        }
    }
} 