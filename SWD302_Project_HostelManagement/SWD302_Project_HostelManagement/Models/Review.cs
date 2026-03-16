using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int TenantId { get; set; }

    public int HostelId { get; set; }

    public int? BookingId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? OwnerReply { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual BookingRequest? BookingRequest { get; set; }

    public virtual Hostel Hostel { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
