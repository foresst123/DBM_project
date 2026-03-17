namespace SWD302_Project_HostelManagement.ViewModels.Auth;

public class RegisterViewModel
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    // "Tenant" hoặc "HostelOwner"
    public string Role { get; set; } = "Tenant";

    // Chỉ dùng khi Role = "HostelOwner"
    public string? BusinessLicense { get; set; }

    // Chỉ dùng khi Role = "Tenant"
    public string? IdentityCard { get; set; }
}
