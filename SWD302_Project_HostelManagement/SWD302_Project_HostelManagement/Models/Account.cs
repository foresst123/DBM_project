using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Status { get; set; } = "Active";

    public string? AvatarUrl { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual Tenant? Tenant { get; set; }

    public virtual HostelOwner? HostelOwner { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<ViolationReport> ViolationReportedAccounts { get; set; } = new List<ViolationReport>();
}
