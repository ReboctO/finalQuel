using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IComplaintRepository : IRepository<Complaint>
    {
        Task<IEnumerable<Complaint>> GetComplaintsByUserAsync(int userId);
        Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(ComplaintStatus status);
        Task<IEnumerable<Complaint>> GetComplaintsByAssigneeAsync(int assigneeId);
        Task<IEnumerable<Complaint>> GetComplaintsByCategoryAsync(ComplaintCategory category);
    }
} 