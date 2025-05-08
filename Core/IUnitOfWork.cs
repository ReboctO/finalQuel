using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IPropertyRepository PropertyRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        
        Task<int> SaveChangesAsync();
    }
} 