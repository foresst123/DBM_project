using System;
using System.Collections.Generic;

namespace SWD302_Project_HostelManagement.Models;

public partial class HostelOwner
{
    public int OwnerId { get; set; }

    // ✅ Thông tin đăng nhập — tự lưu, không qua Account
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Status { get; set; } = "Active";
    public string? AvatarUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // ✅ Thông tin profile riêng của HostelOwner
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? BusinessLicense { get; set; }

    // Navigation properties
    public virtual ICollection<Hostel> Hostels { get; set; }
        = new List<Hostel>();
}
