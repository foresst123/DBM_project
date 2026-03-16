using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class ViolationReport
{
    public int ReportId { get; set; }

    public int ReporterTenantId { get; set; }

    public int? ReportedAccountId { get; set; }

    public int? HostelId { get; set; }

    public string Reason { get; set; } = null!;

    public string? Evidence { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedDate { get; set; }

    public virtual Hostel? Hostel { get; set; }

    public virtual Account? ReportedAccount { get; set; }

    public virtual Tenant Reporter { get; set; } = null!;
}
