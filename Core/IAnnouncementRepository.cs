using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetArchivedAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetScheduledAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetAnnouncementsByCreatorAsync(int createdById);
        Task<IEnumerable<Announcement>> GetAnnouncementsByUrgencyLevelAsync(UrgencyLevel urgencyLevel);
        Task<IEnumerable<AnnouncementRecipient>> GetAnnouncementRecipientsAsync(int announcementId);
        Task AddAnnouncementRecipientAsync(AnnouncementRecipient recipient);
        Task AddAnnouncementRecipientsAsync(IEnumerable<AnnouncementRecipient> recipients);
        Task<bool> MarkAnnouncementAsReadAsync(int announcementId, int userId);
        Task<IEnumerable<Announcement>> GetAnnouncementsForUserAsync(int userId);
    }
} 