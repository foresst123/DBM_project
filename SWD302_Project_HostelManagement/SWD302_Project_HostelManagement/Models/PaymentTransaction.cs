using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class PaymentTransaction
{
    public int TransactionId { get; set; }

    public int BookingId { get; set; }

    public int TenantId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? GatewayRef { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual BookingRequest BookingRequest { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
