using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class RoomUpdateLog
{
    public int LogId { get; set; }

    public int RoomId { get; set; }

    public int? BookingId { get; set; }

    public int? ChangedByOwnerId { get; set; }

    public string StatusBefore { get; set; } = null!;

    public string StatusAfter { get; set; } = null!;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public virtual BookingRequest? BookingRequest { get; set; }

    public virtual HostelOwner? ChangedBy { get; set; }

    public virtual Room Room { get; set; } = null!;
}
