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
            return await Context.Payments
                .Include(p => p.Property)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByPropertyAsync(int propertyId)
        {
            return await Context.Payments
                .Include(p => p.Property)
                .Where(p => p.PropertyId == propertyId)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await Context.Payments
                .Include(p => p.Property)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await Context.Payments
                .Include(p => p.Property)
                .Where(p => p.DueDate >= start && p.DueDate <= end)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
        {
            return await Context.Payments
                .Include(p => p.Property)
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < DateTime.Now)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await Context.Payments.AddAsync(payment);
            return payment;
        }

        public async Task<Payment> GetByIdAsync(int id)
        {
            return await Context.Payments
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        private ApplicationDbContext Context => _context as ApplicationDbContext;
    }
} 