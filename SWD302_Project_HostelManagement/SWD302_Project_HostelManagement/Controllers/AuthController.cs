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

    // ============================================================
    // GET: Auth/Login
    // Hiển thị trang login với 3 nút chọn role
    // ============================================================
    [HttpGet]
    public IActionResult Login(string? role)
    {
        // Nếu đã login thì redirect về Home
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        var model = new LoginViewModel
        {
            Role = role ?? "Tenant"  // default chọn Tenant
        };
        return View(model);
    }

    // ============================================================
    // POST: Auth/Login
    // Kiểm tra đúng bảng tương ứng với Role đã chọn
    // Plain text password comparison (no hashing)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        List<Claim> claims = null;
        string redirectController = "Home";
        string redirectAction = "Index";

        // Chỉ kiểm tra đúng bảng tương ứng với Role được chọn
        if (model.Role == "Tenant")
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Email == model.Email
                                      && t.Status == "Active");

            if (tenant == null || tenant.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tenant.TenantId.ToString()),
                new Claim(ClaimTypes.Email, tenant.Email),
                new Claim(ClaimTypes.Name, tenant.Name ?? tenant.Email),
                new Claim(ClaimTypes.Role, "Tenant")
            };
            redirectController = "Home";
            redirectAction = "Index";
        }
        else if (model.Role == "HostelOwner")
        {
            var owner = await _context.HostelOwners
                .FirstOrDefaultAsync(o => o.Email == model.Email
                                      && o.Status == "Active");

            if (owner == null || owner.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, owner.OwnerId.ToString()),
                new Claim(ClaimTypes.Email, owner.Email),
                new Claim(ClaimTypes.Name, owner.Name ?? owner.Email),
                new Claim(ClaimTypes.Role, "HostelOwner")
            };
            redirectController = "Home";
            redirectAction = "Index";
        }
        else if (model.Role == "Admin")
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == model.Email
                                      && a.Status == "Active");

            if (admin == null || admin.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Name, admin.Name ?? admin.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };
            redirectController = "Home";
            redirectAction = "Index";
        }
        else
        {
            ModelState.AddModelError("", "Invalid role selected.");
            return View(model);
        }

        // Tạo ClaimsIdentity và SignIn
        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            }
        );

        return RedirectToAction(redirectAction, redirectController);
    }

    // ============================================================
    // GET: Auth/Register
    // Hiển thị trang register với 2 nút chọn role (Tenant/HostelOwner)
    // ============================================================
    [HttpGet]
    public IActionResult Register(string? role)
    {
        // Nếu đã login thì redirect về Home
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        var model = new RegisterViewModel
        {
            Role = role ?? "Tenant"  // default chọn Tenant
        };
        return View(model);
    }

    // ============================================================
    // POST: Auth/Register
    // Tạo tài khoản Tenant hoặc HostelOwner
    // Plain text password (no hashing)
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Kiểm tra email đã tồn tại
        bool emailExists = await _context.Tenants.AnyAsync(t => t.Email == model.Email)
                        || await _context.HostelOwners.AnyAsync(o => o.Email == model.Email)
                        || await _context.Admins.AnyAsync(a => a.Email == model.Email);

        if (emailExists)
        {
            ModelState.AddModelError("Email", "Email already exists.");
            return View(model);
        }

        // Tạo tài khoản dựa trên Role được chọn
        if (model.Role == "Tenant")
        {
            var tenant = new Tenant
            {
                Email = model.Email,
                PasswordHash = model.Password,  // Plain text password
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                IdentityCard = model.IdentityCard,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                AvatarUrl = null
            };

            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();

            // Tự động sign in sau khi register
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tenant.TenantId.ToString()),
                new Claim(ClaimTypes.Email, tenant.Email),
                new Claim(ClaimTypes.Name, tenant.Name ?? tenant.Email),
                new Claim(ClaimTypes.Role, "Tenant")
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
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                }
            );

            return RedirectToAction("Index", "Home");
        }
        else if (model.Role == "HostelOwner")
        {
            // Kiểm tra BusinessLicense required cho HostelOwner
            if (string.IsNullOrWhiteSpace(model.BusinessLicense))
            {
                ModelState.AddModelError("BusinessLicense", "Business License is required for Hostel Owner.");
                return View(model);
            }

            var owner = new HostelOwner
            {
                Email = model.Email,
                PasswordHash = model.Password,  // Plain text password
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                BusinessLicense = model.BusinessLicense,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                AvatarUrl = null
            };

            await _context.HostelOwners.AddAsync(owner);
            await _context.SaveChangesAsync();

            // Tự động sign in sau khi register
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, owner.OwnerId.ToString()),
                new Claim(ClaimTypes.Email, owner.Email),
                new Claim(ClaimTypes.Name, owner.Name ?? owner.Email),
                new Claim(ClaimTypes.Role, "HostelOwner")
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
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                }
            );

            return RedirectToAction("Index", "Home");
        }
        else
        {
            ModelState.AddModelError("", "Invalid role selected.");
            return View(model);
        }
    }

    // ============================================================
    // POST: Auth/Logout
    // ============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // GET: /Auth/AccessDenied
    public IActionResult AccessDenied() => View();
}
