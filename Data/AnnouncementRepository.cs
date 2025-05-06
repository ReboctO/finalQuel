using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now))
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetArchivedAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => !a.IsActive || (a.ExpiryDate != null && a.ExpiryDate <= DateTime.Now))
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetScheduledAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.PublishedDate > DateTime.Now)
                .OrderBy(a => a.PublishedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetAnnouncementsByCreatorAsync(int createdById)
        {
            return await _context.Announcements
                .Where(a => a.CreatedById == createdById)
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Announcement>> GetAnnouncementsByUrgencyLevelAsync(UrgencyLevel urgencyLevel)
        {
            return await _context.Announcements
                .Where(a => a.UrgencyLevel == urgencyLevel)
                .OrderByDescending(a => a.PublishedDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<AnnouncementRecipient>> GetAnnouncementRecipientsAsync(int announcementId)
        {
            return await _context.AnnouncementRecipients
                .Where(ar => ar.AnnouncementId == announcementId)
                .Include(ar => ar.User)
                .ToListAsync();
        }
        
        public async Task AddAnnouncementRecipientAsync(AnnouncementRecipient recipient)
        {
            await _context.AnnouncementRecipients.AddAsync(recipient);
            await _context.SaveChangesAsync();
        }
        
        public async Task AddAnnouncementRecipientsAsync(IEnumerable<AnnouncementRecipient> recipients)
        {
            await _context.AnnouncementRecipients.AddRangeAsync(recipients);
            await _context.SaveChangesAsync();
        }
        
        public async Task<bool> MarkAnnouncementAsReadAsync(int announcementId, int userId)
        {
            var recipient = await _context.AnnouncementRecipients
                .FirstOrDefaultAsync(ar => ar.AnnouncementId == announcementId && ar.UserId == userId);
                
            if (recipient == null)
                return false;
                
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Announcement>> GetAnnouncementsForUserAsync(int userId)
        {
            return await _context.AnnouncementRecipients
                .Where(ar => ar.UserId == userId)
                .Include(ar => ar.Announcement)
                .OrderByDescending(ar => ar.Announcement.PublishedDate)
                .Select(ar => ar.Announcement)
                .ToListAsync();
        }
    }
} 