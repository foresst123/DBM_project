using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? BookingId { get; set; }

    public string RecipientEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string MessageContent { get; set; } = null!;

    public string? Type { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public virtual BookingRequest? BookingRequest { get; set; }

    /// <summary>
    /// Creates a new Notification record
    /// </summary>
    /// <param name="bookingId">The booking ID associated with this notification</param>
    /// <param name="recipientEmail">The email address of the recipient</param>
    /// <param name="subject">The subject/type of notification</param>
    /// <returns>A new Notification instance</returns>
    public static Notification CreateRecord(int bookingId, string recipientEmail, string subject)
    {
        return new Notification
        {
            BookingId = bookingId,
            RecipientEmail = recipientEmail,
            Subject = subject,
            MessageContent = "",
            Type = subject,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }
}

