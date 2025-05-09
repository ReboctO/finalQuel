using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Payments = new PaymentRepository(_context);
            Properties = new PropertyRepository(_context);
            Users = new UserRepository(_context);
            Complaints = new ComplaintRepository(_context);
            Events = new EventRepository(_context);
            Announcements = new AnnouncementRepository(_context);
            Facilities = new FacilityRepository(_context);
        }

        public IPaymentRepository Payments { get; private set; }
        public IPropertyRepository Properties { get; private set; }
        public IUserRepository Users { get; private set; }
        public IComplaintRepository Complaints { get; private set; }
        public IEventRepository Events { get; private set; }
        public IAnnouncementRepository Announcements { get; private set; }
        public IFacilityRepository Facilities { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 