using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        
        public UnitOfWork(
            ApplicationDbContext context,
            IUserRepository userRepository,
            IPropertyRepository propertyRepository,
            IPaymentRepository paymentRepository)
        {
            _context = context;
            UserRepository = userRepository;
            PropertyRepository = propertyRepository;
            PaymentRepository = paymentRepository;
        }
        
        public IUserRepository UserRepository { get; }
        public IPropertyRepository PropertyRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
} 