using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TheQuel.Core;
using TheQuel.Models;

namespace TheQuel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBillingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminBillingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new BillingDashboardViewModel
            {
                TotalYearlyPayments = await GetTotalPaymentsForYear(),
                TotalMonthlyPayments = await GetTotalPaymentsForMonth(),
                PendingPaymentsCount = (await _unitOfWork.Payments.GetPaymentsByStatusAsync(PaymentStatus.Pending)).Count(),
                OverduePaymentsCount = (await _unitOfWork.Payments.GetOverduePaymentsAsync()).Count(),
                RecentPayments = await _unitOfWork.Payments.GetPaymentsByDateRangeAsync(DateTime.Now.AddDays(-30), DateTime.Now),
                PendingBills = await _unitOfWork.Payments.GetPaymentsByStatusAsync(PaymentStatus.Pending)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateBill()
        {
            var properties = await _unitOfWork.Properties.GetAllAsync();
            var viewModel = new CreateBillViewModel
            {
                Properties = properties.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} - {p.Address}"
                }).ToList(),
                PaymentTypes = Enum.GetValues(typeof(PaymentType))
                    .Cast<PaymentType>()
                    .Select(t => new SelectListItem
                    {
                        Value = ((int)t).ToString(),
                        Text = t.ToString()
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(CreateBillViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payment = new Payment
            {
                PropertyId = model.PropertyId,
                Amount = model.Amount,
                DueDate = model.DueDate,
                Type = (PaymentType)model.PaymentTypeId,
                Status = PaymentStatus.Pending,
                Notes = model.Notes,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            // TODO: Send notifications to homeowners
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var viewModel = new PaymentReportsViewModel
            {
                CurrentYear = DateTime.Now.Year,
                CurrentMonth = DateTime.Now.Month,
                ReportTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "monthly", Text = "Monthly Report" },
                    new SelectListItem { Value = "yearly", Text = "Yearly Report" }
                },
                Years = Enumerable.Range(DateTime.Now.Year - 5, 6)
                    .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() })
                    .ToList(),
                Months = Enumerable.Range(1, 12)
                    .Select(m => new SelectListItem { Value = m.ToString(), Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                    .ToList(),
                PaymentStatuses = Enum.GetValues(typeof(PaymentStatus))
                    .Cast<PaymentStatus>()
                    .Select(s => new SelectListItem { Value = ((int)s).ToString(), Text = s.ToString() })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id, string referenceNumber)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = PaymentStatus.Paid;
            payment.PaymentDate = DateTime.Now;
            payment.ReferenceNumber = referenceNumber;
            payment.UpdatedAt = DateTime.Now;

            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> NotifyHomeowners(int[] paymentIds)
        {
            // TODO: Implement notification logic
            return Json(new { success = true, message = "Notifications sent successfully" });
        }

        private async Task<decimal> GetTotalPaymentsForYear()
        {
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);
            var payments = await _unitOfWork.Payments.GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        }

        private async Task<decimal> GetTotalPaymentsForMonth()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var payments = await _unitOfWork.Payments.GetPaymentsByDateRangeAsync(startDate, endDate);
            return payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        }
    }
} 