using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.ViewModels.Auth;

namespace SWD302_Project_HostelManagement.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Auth/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: /Auth/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Kiểm tra email đã tồn tại
        if (await _context.Accounts.AnyAsync(a => a.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists.");
            return View(model);
        }

        // Tạo Account — lưu password plaintext
        var account = new Account
        {
            Email = model.Email,
            PasswordHash = model.Password,  // plaintext - KHÔNG hash
            Role = "Tenant",
            Status = "Active",
            CreatedDate = DateTime.UtcNow
        };
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        // Tạo Tenant profile
        var tenant = new Tenant
        {
            AccountId = account.AccountId,
            Name = model.Name,
            PhoneNumber = model.PhoneNumber
        };
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // GET: /Auth/Login
    [HttpGet]
    public IActionResult Login() => View();

    // POST: /Auth/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Tìm account — so sánh password plaintext
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == model.Email
                                   && a.PasswordHash == model.Password);

        if (account == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        if (account.Status != "Active")
        {
            ModelState.AddModelError("", "Your account is inactive or banned.");
            return View(model);
        }

        // Lấy profile tùy role
        string name = account.Email;
        int? profileId = null;

        switch (account.Role)
        {
            case "Tenant":
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.AccountId == account.AccountId);
                name = tenant?.Name ?? name;
                profileId = tenant?.TenantId;
                break;
            case "HostelOwner":
                var owner = await _context.HostelOwners
                    .FirstOrDefaultAsync(o => o.AccountId == account.AccountId);
                name = owner?.Name ?? name;
                profileId = owner?.OwnerId;
                break;
            case "Admin":
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.AccountId == account.AccountId);
                name = admin?.Name ?? name;
                profileId = admin?.AdminId;
                break;
        }

        // Tạo Claims và lưu vào Cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role),
            new Claim(ClaimTypes.Name, name),
            new Claim("AccountId", account.AccountId.ToString()),
            new Claim("ProfileId", profileId?.ToString() ?? "")
        };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
            }
        );

        // Redirect theo role
        return account.Role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "HostelOwner" => RedirectToAction("Index", "Owner"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    // POST: /Auth/Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // GET: /Auth/AccessDenied
    public IActionResult AccessDenied() => View();
}
