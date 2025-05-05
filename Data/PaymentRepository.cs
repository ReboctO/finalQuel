using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByPropertyAsync(int propertyId)
        {
            return await _context.Payments
                .Where(p => p.PropertyId == propertyId)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Payments
                .Where(p => p.DueDate >= start && p.DueDate <= end)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < DateTime.Now)
                .ToListAsync();
        }
    }
} 