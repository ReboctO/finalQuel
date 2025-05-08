using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheQuel.Core;
using System.Linq;

namespace TheQuel.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public PaymentService(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }
        
        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id) 
                ?? throw new InvalidOperationException($"Payment with ID {id} not found");
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByUserAsync(int userId)
        {
            return await _paymentRepository.GetPaymentsByUserAsync(userId);
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByPropertyAsync(int propertyId)
        {
            return await _paymentRepository.GetPaymentsByPropertyAsync(propertyId);
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await _paymentRepository.GetPaymentsByStatusAsync(status);
        }
        
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _paymentRepository.GetPaymentsByDateRangeAsync(start, end);
        }
        
        public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
        {
            return await _paymentRepository.GetOverduePaymentsAsync();
        }
        
        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.Now;
            payment.Status = PaymentStatus.Pending;
            
            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            
            return payment;
        }
        
        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.Now;
            
            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            
            return payment;
        }
        
        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            
            if (payment == null)
            {
                return false;
            }
            
            await _paymentRepository.RemoveAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<Payment> RecordPaymentAsync(int paymentId, DateTime paymentDate, string referenceNumber)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            
            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");
            }
            
            payment.PaymentDate = paymentDate;
            payment.ReferenceNumber = referenceNumber;
            payment.Status = PaymentStatus.Paid;
            payment.UpdatedAt = DateTime.Now;
            
            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            
            return payment;
        }
        
        public async Task<IEnumerable<Payment>> GenerateBillsAsync(PaymentType billType, decimal amount, DateTime dueDate, string notes, bool forAllHomeowners, int? specificUserId = null)
        {
            var createdPayments = new List<Payment>();
            
            // Get users to generate bills for
            IEnumerable<User> homeowners;
            if (forAllHomeowners)
            {
                // Get all users with HomeOwner role and who have a property
                homeowners = await _unitOfWork.UserRepository.GetUsersByRoleAsync(UserRole.HomeOwner);
                homeowners = homeowners.Where(u => u.Property != null);
            }
            else if (specificUserId.HasValue)
            {
                // Get specific user
                var user = await _unitOfWork.UserRepository.GetByIdAsync(specificUserId.Value);
                homeowners = user != null && user.Role == UserRole.HomeOwner && user.Property != null 
                    ? new List<User> { user }
                    : new List<User>();
            }
            else
            {
                return createdPayments; // Return empty list if no valid criteria
            }
            
            // Create a payment for each homeowner
            foreach (var homeowner in homeowners)
            {
                if (homeowner.Property == null) continue;
                
                var payment = new Payment
                {
                    PropertyId = homeowner.Property.Id,
                    UserId = homeowner.Id,
                    Amount = amount,
                    DueDate = dueDate,
                    Status = PaymentStatus.Pending,
                    Type = billType,
                    Notes = notes,
                    CreatedAt = DateTime.Now
                };
                
                await _paymentRepository.AddAsync(payment);
                createdPayments.Add(payment);
            }
            
            await _unitOfWork.SaveChangesAsync();
            return createdPayments;
        }
        
        public async Task<decimal> GetTotalPaymentsForYearAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            
            var payments = await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        }
        
        public async Task<decimal> GetTotalPaymentsForMonthAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var payments = await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        }
        
        public async Task<int> GetPendingPaymentsCountAsync()
        {
            var pendingPayments = await _paymentRepository.GetPaymentsByStatusAsync(PaymentStatus.Pending);
            return pendingPayments.Count();
        }
        
        public async Task<int> GetOverduePaymentsCountAsync()
        {
            var overduePayments = await _paymentRepository.GetOverduePaymentsAsync();
            return overduePayments.Count();
        }
    }
} 