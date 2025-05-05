using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IComplaintService
    {
        Task<Complaint> GetComplaintByIdAsync(int id);
        Task<IEnumerable<Complaint>> GetAllComplaintsAsync();
        Task<IEnumerable<Complaint>> GetComplaintsByUserAsync(int userId);
        Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(ComplaintStatus status);
        Task<IEnumerable<Complaint>> GetComplaintsByAssigneeAsync(int assigneeId);
        Task<IEnumerable<Complaint>> GetComplaintsByCategoryAsync(ComplaintCategory category);
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<Complaint> UpdateComplaintAsync(Complaint complaint);
        Task<bool> DeleteComplaintAsync(int id);
        Task<Complaint> AssignComplaintAsync(int complaintId, int assigneeId);
        Task<Complaint> ResolveComplaintAsync(int complaintId, string resolution);
    }
} 