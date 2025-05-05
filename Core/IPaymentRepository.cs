using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheQuel.Core
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task<IEnumerable<Payment>> GetPaymentsByPropertyAsync(int propertyId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Payment>> GetOverduePaymentsAsync();
    }
} 