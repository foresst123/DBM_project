using SWD302_Project_HostelManagement.VNPay;

namespace SWD302_Project_HostelManagement.Proxies;

/// <summary>
/// PaymentProxy - External Interface Class
/// Information Hiding: Abstracts complexity of payment gateway integration
/// Responsibilities:
///   - Validate input parameters
///   - Forward requests to external Payment Gateway (VNPay)
///   - Handle gateway communication and responses
///   - Log transactions
/// 
/// COMET Pattern: M6-M8
///   M6: Request forwarded to Payment Gateway
///   M7: Payment Gateway returns result
///   M8: Result returned to PaymentCoordinator
/// </summary>
public class PaymentProxy
{
    private readonly ILogger<PaymentProxy> _logger;

    public PaymentProxy(ILogger<PaymentProxy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initiates a payment transaction via external Payment Gateway (VNPay)
    /// 
    /// M6: Chuyển tiếp sang external actor: Payment Gateway
    /// </summary>
    /// <param name="bookingId">Booking identifier (must be > 0)</param>
    /// <param name="amount">Payment amount in VND (must be > 0)</param>
    /// <returns>Payment URL string to redirect user to VNPay</returns>
    /// <exception cref="ArgumentException">If bookingId or amount is invalid</exception>
    public string InitiateTransaction(int bookingId, decimal amount)
    {
        try
        {
            // Precondition: Validate input parameters
            if (bookingId <= 0)
            {
                _logger.LogError("Invalid bookingId: {BookingId}", bookingId);
                throw new ArgumentException("bookingId must be greater than 0", nameof(bookingId));
            }

            if (amount <= 0)
            {
                _logger.LogError("Invalid amount: {Amount}", amount);
                throw new ArgumentException("amount must be greater than 0", nameof(amount));
            }

            _logger.LogInformation(
                "M6: Initiating payment transaction to Payment Gateway: bookingId={BookingId}, amount={Amount}",
                bookingId, amount);

            // M6: Forward to Payment Gateway (external actor)
            // VNPayHelper.CreatePaymentUrl() communicates with external VNPay service
            string paymentUrl = VNPayHelper.CreatePaymentUrl(
                amount,
                bookingId,
                $"Booking #{bookingId} Payment"
            );

            if (string.IsNullOrWhiteSpace(paymentUrl))
            {
                _logger.LogError(
                    "M7A.1: Payment Gateway failed to generate payment URL for bookingId={BookingId}",
                    bookingId);
                throw new InvalidOperationException("Failed to generate payment URL from VNPay");
            }

            // M7 [Successful]: Payment Gateway returned payment URL
            _logger.LogInformation(
                "M7: Payment Gateway confirmed: payment URL generated for booking {BookingId}",
                bookingId);

            // M8: Return result to PaymentCoordinator
            return paymentUrl;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Input validation error for bookingId={BookingId}, amount={Amount}",
                bookingId, amount);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "M7A.1: Error communicating with Payment Gateway for bookingId={BookingId}",
                bookingId);
            throw;
        }
    }
}
