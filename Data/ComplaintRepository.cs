using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class ComplaintRepository : Repository<Complaint>, IComplaintRepository
    {
        public ComplaintRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Complaint>> GetComplaintsByUserAsync(int userId)
        {
            return await _context.Complaints
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(ComplaintStatus status)
        {
            return await _context.Complaints
                .Where(c => c.Status == status)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Complaint>> GetComplaintsByAssigneeAsync(int assigneeId)
        {
            return await _context.Complaints
                .Where(c => c.AssignedToId == assigneeId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Complaint>> GetComplaintsByCategoryAsync(ComplaintCategory category)
        {
            return await _context.Complaints
                .Where(c => c.Category == category)
                .ToListAsync();
        }
    }
} 