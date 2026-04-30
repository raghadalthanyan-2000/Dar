using System.Security.Claims;
using dar_system.Data;
using dar_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

public abstract class AppController(DarDbContext dbContext) : Controller
{
    protected DarDbContext DbContext { get; } = dbContext;

    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

    protected async Task<User?> GetCurrentUserAsync()
    {
        var email = CurrentUserEmail;
        return string.IsNullOrWhiteSpace(email)
            ? null
            : await DbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    protected async Task<Client?> GetCurrentClientAsync()
    {
        var email = CurrentUserEmail;
        return string.IsNullOrWhiteSpace(email)
            ? null
            : await DbContext.Clients.FirstOrDefaultAsync(c => c.Email == email);
    }

    protected async Task<Designer?> GetCurrentDesignerAsync()
    {
        var email = CurrentUserEmail;
        return string.IsNullOrWhiteSpace(email)
            ? null
            : await DbContext.Designers.FirstOrDefaultAsync(d => d.Email == email);
    }

    protected async Task<Administrator?> GetCurrentAdministratorAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.EntityId is null
            ? null
            : await DbContext.Administrators.FirstOrDefaultAsync(a => a.AdminId == user.EntityId);
    }
}
