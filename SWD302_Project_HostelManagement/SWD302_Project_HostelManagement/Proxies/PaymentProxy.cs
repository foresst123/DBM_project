using System.Text;
using System.Text.Json;

namespace SWD302_Project_HostelManagement.Proxies
{
    public class PaymentProxy
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentProxy> _logger;

        public PaymentProxy(IConfiguration configuration, ILogger<PaymentProxy> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Initiates a payment transaction via external Payment Gateway
        /// </summary>
        /// <param name="bookingId">The booking ID</param>
        /// <param name="amount">The amount to be paid</param>
        /// <returns>"Success" if initiated successfully, "Failed" otherwise</returns>
        public string InitiateTransaction(int bookingId, decimal amount)
        {
            try
            {
                // Validate input
                if (bookingId <= 0 || amount <= 0)
                {
                    _logger.LogError("Invalid bookingId or amount: bookingId={BookingId}, amount={Amount}", 
                        bookingId, amount);
                    return "Failed";
                }

                // Forward to Payment Gateway (external actor)
                return SendToPaymentGateway(bookingId, amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating transaction for bookingId={BookingId}", bookingId);
                return "Failed";
            }
        }

        /// <summary>
        /// Sends payment request to Payment Gateway (VNPAY)
        /// </summary>
        private string SendToPaymentGateway(int bookingId, decimal amount)
        {
            try
            {
                // Read configuration
                string gatewayUrl = _configuration["PaymentSettings:GatewayUrl"];
                string apiKey = _configuration["PaymentSettings:ApiKey"];

                if (string.IsNullOrWhiteSpace(gatewayUrl) || string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("Payment gateway configuration is incomplete");
                    return "Failed";
                }

                // Create HttpClient directly (NOT injected)
                using var httpClient = new HttpClient();

                // Prepare request payload
                var payload = new
                {
                    bookingId = bookingId,
                    amount = amount,
                    timestamp = DateTime.UtcNow
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                // Add API key header
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                // Send POST request to payment gateway
                var response = httpClient.PostAsync(
                    $"{gatewayUrl}/processTransaction",
                    jsonContent
                ).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Payment gateway accepted transaction: bookingId={BookingId}, amount={Amount}",
                        bookingId, amount);
                    return "Success";
                }
                else
                {
                    _logger.LogError(
                        "Payment gateway rejected transaction: bookingId={BookingId}, statusCode={StatusCode}",
                        bookingId, response.StatusCode);
                    return "Failed";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error communicating with payment gateway");
                return "Failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending to payment gateway");
                return "Failed";
            }
        }
    }
}
