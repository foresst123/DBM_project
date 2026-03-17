using System.Net;
using System.Net.Mail;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Proxies
{
    public class EmailProxy
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailProxy> _logger;

        public EmailProxy(IConfiguration configuration, ILogger<EmailProxy> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends email for a notification
        /// </summary>
        /// <param name="recipientEmail">The email address of the recipient</param>
        /// <param name="notification">The notification containing the message content</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        public bool SendEmail(string recipientEmail, Notification notification)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(recipientEmail) || !IsValidEmail(recipientEmail))
            {
                _logger.LogError("Invalid recipient email: {Email}", recipientEmail);
                return false;
            }

            var messageContent = notification.MessageContent;

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogError("Notification content is empty");
                return false;
            }

            // Chuyển tiếp sang external actor: Email Delivery Service (SmtpClient)
            return SendViaEmailDeliveryService(recipientEmail, notification.Subject, messageContent);
        }

        /// <summary>
        /// Sends email via SMTP (Email Delivery Service)
        /// </summary>
        private bool SendViaEmailDeliveryService(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                // Email Delivery Service = SmtpClient (external actor)
                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                    Timeout = 10000
                };

                smtpClient.Send(new MailMessage(senderEmail, to, subject, body) { IsBodyHtml = true });

                _logger.LogInformation("Email Delivery Service confirmed: sent to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email Delivery Service returned failure for: {To}", to);
                return false;
            }
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                return new MailAddress(email).Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

