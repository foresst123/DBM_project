using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;
using SWD302_Project_HostelManagement.VNPay;

namespace SWD302_Project_HostelManagement.Controllers;

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

    // ================================================================
    // COMET: initiateDepositPayment(tenantId, bookingId, amount) : String
    // ================================================================
    // M3: Booking Eligibility Request
    // M4: Booking Payment Eligibility Information
    // M5: Transaction Initiation Request
    // M6→M8: PaymentProxy → Payment Gateway
    // Return: Payment URL để redirect browser
    // ================================================================
    [HttpPost("InitiatePayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiateDepositPayment(int bookingId, decimal amount)
    {
        try
        {
            // Lấy tenantId từ Claims
            var tenantIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(tenantIdStr))
            {
                TempData["Error"] = "You must be logged in to make a payment.";
                return RedirectToAction("Login", "Auth");
            }

            var tenantId = int.Parse(tenantIdStr);

            _logger.LogInformation(
                "Payment initiated: TenantId={TenantId}, BookingId={BookingId}, Amount={Amount}",
                tenantId, bookingId, amount);

            // M3: Find booking
            var booking = await _context.BookingRequests
                .FirstOrDefaultAsync(b => b.BookingId == bookingId
                                       && b.TenantId == tenantId);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found: BookingId={BookingId}, TenantId={TenantId}",
                    bookingId, tenantId);
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("BookingRequestIndex", "Tenant");
            }

            // M4: Check booking eligibility
            if (booking.Status != "PendingPayment")
            {
                _logger.LogWarning(
                    "Booking not eligible for payment: BookingId={BookingId}, Status={Status}",
                    bookingId, booking.Status);
                TempData["Error"] = $"This booking is not eligible for payment. BookingId={bookingId}, Status={booking.Status}";
                return RedirectToAction("BookingRequestIndex", "Tenant");
            }

            // M5→M8: Call PaymentProxy → Payment Gateway
            // PaymentProxy.InitiateTransaction() forwards to external VNPay
            // Returns: Payment URL if successful, throws exception if failed
            string paymentUrl;
            try
            {
                paymentUrl = _paymentProxy.InitiateTransaction(bookingId, amount);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid payment parameters: bookingId={BookingId}, amount={Amount}",
                    bookingId, amount);
                TempData["Error"] = "Invalid payment parameters.";
                return RedirectToAction("BookingRequestIndex", "Tenant");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Payment Gateway error for booking {BookingId}", bookingId);
                TempData["Error"] = "Cannot connect to Payment Gateway. Please try again later.";
                return RedirectToAction("BookingRequestIndex", "Tenant");
            }

            if (string.IsNullOrEmpty(paymentUrl))
            {
                _logger.LogError("Payment URL is null or empty for booking {BookingId}", bookingId);
                TempData["Error"] = "Cannot create payment URL.";
                return RedirectToAction("BookingRequestIndex", "Tenant");
            }

            _logger.LogInformation(
                "M8: Redirecting to Payment Gateway for booking {BookingId}. URL: {PaymentUrl}",
                bookingId, paymentUrl);

            // Redirect browser to Payment Gateway (VNPay)
            return Redirect(paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in initiateDepositPayment for bookingId={BookingId}", bookingId);
            TempData["Error"] = "An error occurred while initiating payment.";
            return RedirectToAction("BookingRequestIndex", "Tenant");
        }
    }

    // ================================================================
    // COMET: processPaymentResult(bookingId, isSuccess, amountPaid, gatewayRef)
    // ================================================================
    // Called by VNPay as callback (GET request with query parameters)
    // Process payment result: success → update status, send email
    //                         failure → create notification, send email
    // ================================================================

    /// <summary>
    /// VNPay callback endpoint (matches ReturnUrl in appsettings.json)
    /// Routes to ProcessPaymentResult for actual processing
    /// </summary>
    [HttpGet("VNPayReturn")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn()
    {
        _logger.LogInformation("VNPayReturn callback received with query: {Query}", Request.QueryString);
        return await ProcessPaymentResult();
    }

    [HttpGet("ProcessPaymentResult")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPaymentResult()
    {
        try
        {
            var queryParams = Request.Query;

            // Validate VNPay signature để đảm bảo callback từ VNPay thật
            var lib = new VnPayLibrary();
            foreach (var (key, value) in queryParams)
                lib.AddResponseData(key, value.ToString());

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = lib.ValidateSignature(
                vnpSecureHash,
                VNPayConfig.HashSecret);

            if (!isValidSignature)
            {
                _logger.LogWarning("Invalid VNPay signature");
                TempData["Error"] = "Invalid payment response signature.";
                return RedirectToAction("Index", "Home");
            }

            // Parse VNPay response
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString();
            var txnRef = queryParams["vnp_TxnRef"].ToString();
            var amountStr = queryParams["vnp_Amount"].ToString();
            var gatewayRef = queryParams["vnp_TransactionNo"].ToString();

            // Extract bookingId từ TxnRef format: BOOKING_[bookingId]_[timestamp]
            int bookingId = int.Parse(txnRef.Split('_')[1]);
            decimal amountPaid = decimal.Parse(amountStr) / 100;  // VNPay gửi amount * 100
            bool isSuccess = transactionStatus == "00";  // "00" = VNPay success code

            _logger.LogInformation(
                "Payment result received: BookingId={BookingId}, IsSuccess={IsSuccess}, Amount={Amount}",
                bookingId, isSuccess, amountPaid);

            // Gọi method xử lý kết quả
            await ProcessPaymentResultInternal(bookingId, isSuccess, amountPaid, gatewayRef);

            return RedirectToAction("BookingRequestIndex", "Tenant");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPaymentResult");
            TempData["Error"] = "An error occurred while processing the payment result.";
            return RedirectToAction("Index", "Home");
        }
    }

    // ================================================================
    // Internal method to process payment result (success or failure)
    // ================================================================
    private async Task ProcessPaymentResultInternal(
        int bookingId,
        bool isSuccess,
        decimal amountPaid,
        string gatewayRef)
    {
        // Lấy thông tin booking
        var booking = await _context.BookingRequests
            .Include(b => b.Tenant)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            _logger.LogError("Booking not found: {BookingId}", bookingId);
            TempData["Error"] = "Booking not found.";
            return;
        }

        // M10→M11: Lấy tenant email
        int tenantId = booking.TenantId;
        var tenant = booking.Tenant;
        if (tenant == null)
        {
            _logger.LogError("Tenant not found: {TenantId}", tenantId);
            TempData["Error"] = "Tenant not found.";
            return;
        }

        string email = tenant.Email;

        if (isSuccess)
        {
            // ========== SUCCESS FLOW ==========

            // M9: Update Status = "Deposit Paid"
            booking.Status = "DepositPaid";
            booking.UpdatedDate = DateTime.UtcNow;

            // M12: Create PaymentTransaction record
            var transaction = new PaymentTransaction
            {
                BookingId = bookingId,
                TenantId = tenantId,
                Amount = amountPaid,
                Status = "Success",
                PaymentMethod = "VNPay",
                GatewayRef = gatewayRef,
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
                MessageContent = $"Your payment of {amountPaid:N0} VND for booking #{bookingId} " +
                                 "has been successfully processed. Your booking is now confirmed.",
                Type = "PaymentSuccess",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            // Save changes
            await _context.SaveChangesAsync();

            // M14→M17: Send email via EmailProxy
            bool emailSent = _emailProxy.SendEmail(email, notification);

            if (emailSent)
            {
                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Success email sent to {Email}", email);
            }

            TempData["Success"] = "Payment successful! Your booking is now confirmed.";
        }
        else
        {
            // ========== FAILURE FLOW (AS 16.1) ==========

            // M7A.4: Create Notification record [Failure]
            var notification = new Notification
            {
                BookingId = bookingId,
                RecipientEmail = email,
                Subject = "Payment Failed - Please try again",
                MessageContent = $"Your payment for booking #{bookingId} was not successful. " +
                                 "Please try to pay again or contact our support team.",
                Type = "PaymentFailed",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            // Save notification
            await _context.SaveChangesAsync();

            // M7A.5→M7A.8: Send failure email via EmailProxy
            if (!string.IsNullOrWhiteSpace(email))
            {
                bool emailSent = _emailProxy.SendEmail(email, notification);

                if (emailSent)
                {
                    notification.Status = "Sent";
                    notification.SentAt = DateTime.UtcNow;
                    _context.Notifications.Update(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Failure email sent to {Email}", email);
                }
            }

            TempData["Error"] = "Payment failed. Please try again or contact our support.";
        }
    }
}
