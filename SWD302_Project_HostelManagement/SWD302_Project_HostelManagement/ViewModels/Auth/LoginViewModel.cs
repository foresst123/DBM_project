namespace SWD302_Project_HostelManagement.ViewModels.Auth;

public class LoginViewModel
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    // "Tenant", "HostelOwner", "Admin"
    public string Role { get; set; } = "Tenant";
}
