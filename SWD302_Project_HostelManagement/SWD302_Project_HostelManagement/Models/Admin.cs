using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class Admin
{
    public int AdminId { get; set; }

    public int AccountId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;
}
