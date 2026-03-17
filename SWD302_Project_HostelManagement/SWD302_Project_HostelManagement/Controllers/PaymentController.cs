using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;

namespace SWD302_Project_HostelManagement.Controllers
{
    [Route("Payment")]
    [Authorize(Roles = "Tenant")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaymentProxy _paymentProxy;
        private readonly EmailProxy _emailProxy;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            AppDbContext context,
            PaymentProxy paymentProxy,
            EmailProxy emailProxy,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _paymentProxy = paymentProxy;
            _emailProxy = emailProxy;
            _logger = logger;
        }

        /// <summary>
        /// Initiates a payment transaction
        /// </summary>
        [HttpPost("InitiatePayment")]
        public IActionResult InitiatePayment(int bookingId, decimal amount)
        {
            try
            {
                // Call PaymentProxy to initiate transaction
                string result = _paymentProxy.InitiateTransaction(bookingId, amount);

                if (result == "Success")
                {
                    // Redirect to ProcessPaymentResult with success
                    return ProcessPaymentResult(bookingId, "Success", amount);
                }
                else
                {
                    // Redirect to ProcessPaymentResult with failure
                    return ProcessPaymentResult(bookingId, "Failed", amount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for bookingId={BookingId}", bookingId);
                return BadRequest("An error occurred while initiating payment.");
            }
        }

        /// <summary>
        /// Processes payment result according to COMET pseudocode
        /// </summary>
        [HttpPost("ProcessPaymentResult")]
        [Route("ProcessPaymentResult")]
        public IActionResult ProcessPaymentResult(int bookingId, string transactionStatus, decimal amountPaid)
        {
            try
            {
                // M3: find BookingRequest (include Tenant navigation)
                var booking = _context.BookingRequests
                    .Include(b => b.Tenant)
                    .FirstOrDefault(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", bookingId);
                    return BadRequest("Booking not found.");
                }

                // Check eligibility
                if (booking.Status != "PendingPayment")
                {
                    _logger.LogWarning("Booking {BookingId} is not eligible for payment. Current status: {Status}",
                        bookingId, booking.Status);
                    return BadRequest("This booking is not eligible for payment.");
                }

                if (transactionStatus == "Success")
                {
                    // M9: Update booking status
                    booking.Status = "DepositPaid";
                    booking.UpdatedDate = DateTime.UtcNow;

                    // M10→M11: Get tenant email
                    int tenantId = booking.TenantId;
                    var tenant = _context.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

                    if (tenant == null)
                    {
                        _logger.LogError("Tenant not found: {TenantId}", tenantId);
                        return BadRequest("Tenant not found.");
                    }

                    string email = tenant.Email;

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        _logger.LogError("Email not found for tenant: {TenantId}", tenantId);
                        return BadRequest("Email not found.");
                    }

                    // M12: Create PaymentTransaction record
                    var transaction = new PaymentTransaction
                    {
                        BookingId = bookingId,
                        TenantId = tenantId,
                        Amount = amountPaid,
                        Status = "Success",
                        PaymentMethod = "Gateway",
                        GatewayRef = Guid.NewGuid().ToString(),
                        PaidAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.PaymentTransactions.Add(transaction);

                    // M13: Create Notification record
                    var notification = new Notification
                    {
                        BookingId = bookingId,
                        RecipientEmail = email,
                        Subject = "Payment Successful - Booking Confirmed",
                        MessageContent = $"Your payment for booking #{bookingId} was successful. Your deposit has been confirmed.",
                        Type = "PaymentSuccess",
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);

                    // Save all changes to database
                    _context.SaveChanges();

                    // M14→M17: Send email via EmailProxy
                    bool emailSent = _emailProxy.SendEmail(email, notification);

                    if (emailSent)
                    {
                        notification.Status = "Sent";
                        notification.SentAt = DateTime.UtcNow;
                        _context.Notifications.Update(notification);
                        _context.SaveChanges();
                        _logger.LogInformation("Payment success email sent to {Email}", email);
                    }

                    // M18→M19: Return success result
                    _logger.LogInformation("Payment processed successfully for booking {BookingId}", bookingId);
                    return Ok(new { message = "Payment processed successfully.", bookingId = bookingId });
                }
                else
                {
                    // AS 16.1: Transaction Failed or Cancelled

                    // M7A.2→M7A.3: Get tenant email
                    int tenantId = booking.TenantId;
                    var tenant = _context.Tenants.FirstOrDefault(t => t.TenantId == tenantId);

                    if (tenant == null)
                    {
                        _logger.LogError("Tenant not found: {TenantId}", tenantId);
                        return BadRequest("Tenant not found.");
                    }

                    string email = tenant.Email;

                    // M7A.4: Create Notification record [Failure]
                    var notification = new Notification
                    {
                        BookingId = bookingId,
                        RecipientEmail = email,
                        Subject = "Payment Failed",
                        MessageContent = $"Your payment for booking #{bookingId} failed. Please try again.",
                        Type = "PaymentFailed",
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);

                    // Save notification to database
                    _context.SaveChanges();

                    // M7A.5→M7A.8: Send failure email via EmailProxy
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        bool emailSent = _emailProxy.SendEmail(email, notification);

                        if (emailSent)
                        {
                            notification.Status = "Sent";
                            notification.SentAt = DateTime.UtcNow;
                            _context.Notifications.Update(notification);
                            _context.SaveChanges();
                            _logger.LogInformation("Payment failure email sent to {Email}", email);
                        }
                    }

                    // M7A.9→M7A.10: Return failure result
                    _logger.LogWarning("Payment failed for booking {BookingId}. Transaction status: {Status}",
                        bookingId, transactionStatus);
                    return BadRequest(new { message = "Payment was not successful or was cancelled.", bookingId = bookingId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment result for booking {BookingId}", bookingId);
                return StatusCode(500, "An error occurred while processing the payment.");
            }
        }
    }
}
