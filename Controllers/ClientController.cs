using BCrypt.Net;
using dar_system.Data;
using dar_system.DTOs;
using dar_system.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize]
[Route("client")]
public class ClientController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var recentRequests = await DbContext.DesignRequests.Where(r => r.ClientId == client.ClientId).OrderByDescending(r => r.DateTime).Take(5).ToListAsync();
        var upcomingConsultations = await DbContext.Consultations.Include(c => c.Designer).Where(c => c.ClientId == client.ClientId && c.ConsultationStatus == "scheduled" && c.ScheduledAt > DateTime.UtcNow).OrderBy(c => c.ScheduledAt).Take(5).ToListAsync();

        return View("~/Views/Client/dashboard.cshtml", new
        {
            Client = client,
            TotalRequests = await DbContext.DesignRequests.CountAsync(r => r.ClientId == client.ClientId),
            PendingRequests = await DbContext.DesignRequests.CountAsync(r => r.ClientId == client.ClientId && r.Status == "pending"),
            TotalConsultations = await DbContext.Consultations.CountAsync(c => c.ClientId == client.ClientId),
            UpcomingConsultations = await DbContext.Consultations.CountAsync(c => c.ClientId == client.ClientId && c.ConsultationStatus == "scheduled" && c.ScheduledAt > DateTime.UtcNow),
            RecentRequests = recentRequests,
            UpcomingConsultationsList = upcomingConsultations
        });
    }

    [HttpGet("my-consultations")]
    public async Task<IActionResult> MyConsultations(string? status)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var query = DbContext.Consultations.Include(c => c.Designer).Where(c => c.ClientId == client.ClientId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.ConsultationStatus == status);
        var data = await query.OrderByDescending(c => c.ScheduledAt).ToListAsync();
        return View("~/Views/Client/myConsultations.cshtml", data);
    }

    [HttpGet("my-payments")]
    public async Task<IActionResult> MyPayments(string? status)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var query = DbContext.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Consultation!).ThenInclude(c => c!.Designer)
            .Include(p => p.Project!).ThenInclude(pr => pr!.Proposal).ThenInclude(pp => pp.DesignRequest)
            .Where(p => (p.Consultation != null && p.Consultation.ClientId == client.ClientId) ||
                        (p.Project != null && p.Project.Proposal.DesignRequest.ClientId == client.ClientId));

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.PaymentStatus == status);

        var data = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return View("~/Views/Client/myPayments.cshtml", data);
    }

    [HttpGet("track-request/{requestId:int}")]
    public async Task<IActionResult> TrackRequest(int requestId)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var request = await DbContext.DesignRequests
            .Include(r => r.Designer)
            .Include(r => r.Proposals).ThenInclude(p => p.Designer)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.ClientId == client.ClientId);

        return request is null ? NotFound() : View("~/Views/Client/trackRequest.cshtml", request);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var model = new ClientProfileDto
        {
            ClientId = client.ClientId,
            FirstName = client.FirstName,
            LastName = client.LastName,
            YearOfBirth = client.YearOfBirth,
            Phone = client.Phone,
            Email = client.Email
        };
        return View(model);
    }

    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ClientProfileDto model)
    {
        var client = await GetCurrentClientAsync();
        var user = await GetCurrentUserAsync();
        if (client is null || user is null) return Challenge();

        if (!ModelState.IsValid) return View(model);

        client.FirstName = model.FirstName;
        client.LastName = model.LastName;
        client.YearOfBirth = model.YearOfBirth;
        client.Phone = model.Phone;
        client.Email = model.Email;
        user.FullName = $"{model.FirstName} {model.LastName}".Trim();
        user.Email = model.Email;

        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpDelete("delete-account")]
    [HttpPost("delete-account")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(PasswordChangeViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
        {
            TempData["ErrorMessage"] = "Current password is incorrect.";
            return RedirectToAction(nameof(Profile));
        }

        TempData["SuccessMessage"] = "Account deletion flow converted. Add hard-delete policy before enabling destructive deletion.";
        return RedirectToAction("Index", "Home");
    }
}
