using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class BookingRequest
{
    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public string RequestType { get; set; } = "Booking";

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Status { get; set; } = "Pending";

    public string? RejectReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual Room Room { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<RoomUpdateLog> RoomUpdateLogs { get; set; } = new List<RoomUpdateLog>();
}
