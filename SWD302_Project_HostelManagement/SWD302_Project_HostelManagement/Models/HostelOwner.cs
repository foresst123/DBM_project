using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class HostelOwner
{
    public int OwnerId { get; set; }

    public int AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? BusinessLicense { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Hostel> Hostels { get; set; } = new List<Hostel>();
}
