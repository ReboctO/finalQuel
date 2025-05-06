using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;
using TheQuel.Models;

namespace TheQuel.Services
{
    public interface IAnnouncementService
    {
        Task<Announcement> GetAnnouncementByIdAsync(int id);
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetArchivedAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetScheduledAnnouncementsAsync();
        Task<Announcement> CreateAnnouncementAsync(AnnouncementCreateViewModel model, int createdById);
        Task<Announcement> UpdateAnnouncementAsync(Announcement announcement);
        Task<bool> DeleteAnnouncementAsync(int id);
        Task<bool> ArchiveAnnouncementAsync(int id);
        Task<bool> PublishAnnouncementAsync(int id);
        Task<bool> RestoreAnnouncementAsync(int id);
        Task<bool> MarkAnnouncementAsReadAsync(int announcementId, int userId);
        Task<IEnumerable<Announcement>> GetAnnouncementsForUserAsync(int userId);
        Task<int> SendAnnouncementNotificationsAsync(int announcementId);
    }
} 