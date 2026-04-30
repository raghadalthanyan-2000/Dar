using System.Security.Claims;
using BCrypt.Net;
using dar_system.Data;
using dar_system.Models;
using dar_system.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

public class AuthController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("login")]
    public IActionResult ShowLogin() => View("~/Views/Auth/Login.cshtml", new LoginViewModel());

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View("~/Views/Auth/Login.cshtml", model);

        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError(nameof(model.Email), "These credentials do not match our records.");
            return View("~/Views/Auth/Login.cshtml", model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.UserType),
            new("user_id", user.UserId.ToString())
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return user.UserType switch
        {
            "admin" => RedirectToAction("Index", "AdminDashboard"),
            "designer" => RedirectToAction("Index", "DesignerHome"),
            _ => RedirectToAction("Dashboard", "Client")
        };
    }

    [HttpGet("register")]
    public IActionResult ShowRegister() => View("~/Views/Auth/Register.cshtml", new RegisterViewModel());

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.UserType == "designer")
        {
            if (string.IsNullOrWhiteSpace(model.Specialty)) ModelState.AddModelError(nameof(model.Specialty), "Specialty is required.");
            if (!model.ExperienceYears.HasValue) ModelState.AddModelError(nameof(model.ExperienceYears), "Experience years is required.");
            if (string.IsNullOrWhiteSpace(model.Bio)) ModelState.AddModelError(nameof(model.Bio), "Bio is required.");
        }

        if (!ModelState.IsValid) return View("~/Views/Auth/Register.cshtml", model);
        if (await DbContext.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            return View("~/Views/Auth/Register.cshtml", model);
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
        int entityId;
        string fullName;

        if (model.UserType == "designer")
        {
            var designer = new Designer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                YearOfBirth = model.YearOfBirth,
                Phone = model.Phone,
                Email = model.Email,
                Password = hashedPassword,
                Bio = model.Bio,
                ExperienceYears = model.ExperienceYears,
                Specialty = model.Specialty,
                VerificationStatus = "pending",
                RatingAverage = 0,
                RegisteredAt = DateTime.UtcNow
            };
            DbContext.Designers.Add(designer);
            await DbContext.SaveChangesAsync();
            entityId = designer.DesignerId;
            fullName = designer.FullName;
        }
        else
        {
            var client = new Client
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                YearOfBirth = model.YearOfBirth,
                Phone = model.Phone,
                Email = model.Email,
                Password = hashedPassword,
                RegisteredAt = DateTime.UtcNow
            };
            DbContext.Clients.Add(client);
            await DbContext.SaveChangesAsync();
            entityId = client.ClientId;
            fullName = client.FullName;
        }

        var user = new User { FullName = fullName, Email = model.Email, Password = hashedPassword, UserType = model.UserType, EntityId = entityId };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return await Login(new LoginViewModel { Email = model.Email, Password = model.Password });
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
