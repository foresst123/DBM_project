namespace SWD302_Project_HostelManagement.ViewModels.Auth;

public class RegisterViewModel
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}
