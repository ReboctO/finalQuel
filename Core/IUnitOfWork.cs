using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IUnitOfWork
    {
        IPaymentRepository Payments { get; }
        IPropertyRepository Properties { get; }
        IUserRepository Users { get; }
        IComplaintRepository Complaints { get; }
        IEventRepository Events { get; }
        IAnnouncementRepository Announcements { get; }
        IFacilityRepository Facilities { get; }
        Task<int> CompleteAsync();
    }
} 