using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Tenant
{
    public int TenantId { get; set; }

    public int AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? IdentityCard { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<BookingRequest> BookingRequests { get; set; } = new List<BookingRequest>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<ViolationReport> ViolationReports { get; set; } = new List<ViolationReport>();
}
