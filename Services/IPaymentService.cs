using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Services
{
    public interface IPaymentService
    {
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId);
        Task<IEnumerable<Payment>> GetPaymentsByPropertyAsync(int propertyId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Payment>> GetOverduePaymentsAsync();
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentAsync(int id);
        Task<Payment> RecordPaymentAsync(int paymentId, DateTime paymentDate, string referenceNumber);
    }
} 