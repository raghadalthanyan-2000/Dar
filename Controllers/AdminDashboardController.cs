using BCrypt.Net;
using dar_system.Data;
using dar_system.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "admin")]
[Route("admin")]
public class AdminDashboardController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        var model = new
        {
            TotalUsers = await DbContext.Users.CountAsync(),
            TotalClients = await DbContext.Clients.CountAsync(),
            TotalDesigners = await DbContext.Designers.CountAsync(),
            PendingDesigners = await DbContext.Designers.CountAsync(d => d.VerificationStatus == "pending"),
            ActiveRequests = await DbContext.DesignRequests.CountAsync(r => r.Status == "pending" || r.Status == "accepted" || r.Status == "in_progress"),
            ScheduledConsultations = await DbContext.Consultations.CountAsync(c => c.ConsultationStatus == "approved" || c.ConsultationStatus == "scheduled"),
            ActiveProjects = await DbContext.Projects.CountAsync(p => p.ProjectStatus == "in_progress"),
            CompletedPayments = await DbContext.Payments.CountAsync(p => p.PaymentStatus == "completed")
        };
        return View("~/Views/Admin/dashboard.cshtml", model);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var admin = await GetCurrentAdministratorAsync();
        var user = await GetCurrentUserAsync();
        return View("~/Views/Admin/profile.cshtml", new { Admin = admin, User = user });
    }

    [HttpPost("profile")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string email)
    {
        var admin = await GetCurrentAdministratorAsync();
        var user = await GetCurrentUserAsync();
        if (admin is null || user is null) return Challenge();
        admin.FirstName = firstName;
        admin.LastName = lastName;
        admin.Email = email;
        user.FullName = $"{firstName} {lastName}".Trim();
        user.Email = email;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("profile/password")]
    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword(PasswordChangeViewModel model)
    {
        var admin = await GetCurrentAdministratorAsync();
        var user = await GetCurrentUserAsync();
        if (admin is null || user is null) return Challenge();
        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password)) return BadRequest();
        var hashed = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.Password = hashed;
        admin.Password = hashed;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("designers")]
    public async Task<IActionResult> Designers(string? status, string? search, string? specialty) =>
        View("~/Views/Admin/designers/index.cshtml", await DbContext.Designers.Where(d => (status == null || d.VerificationStatus == status) && (specialty == null || d.Specialty == specialty) && (search == null || d.FirstName.Contains(search) || d.LastName.Contains(search) || d.Email.Contains(search))).OrderByDescending(d => d.RegisteredAt).ToListAsync());

    [HttpPatch("designers/{designerId:int}/status")]
    [HttpPost("designers/{designerId:int}/status")]
    public async Task<IActionResult> UpdateDesignerStatus(int designerId, string verificationStatus)
    {
        var designer = await DbContext.Designers.FindAsync(designerId);
        if (designer is null) return NotFound();
        designer.VerificationStatus = verificationStatus;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Designers));
    }

    [HttpGet("portfolios")]
    public async Task<IActionResult> Portfolios(string? status, string? search) =>
        View("~/Views/Admin/portfolios/index.cshtml", await DbContext.Portfolios.Include(p => p.Designer).Where(p => (status == null || p.ApprovalStatus == status) && (search == null || p.Title.Contains(search) || (p.Description ?? "").Contains(search))).OrderByDescending(p => p.PortfolioId).ToListAsync());

    [HttpPatch("portfolios/{portfolioId:int}/status")]
    [HttpPost("portfolios/{portfolioId:int}/status")]
    public async Task<IActionResult> UpdatePortfolioStatus(int portfolioId, string approvalStatus)
    {
        var portfolio = await DbContext.Portfolios.FindAsync(portfolioId);
        var user = await GetCurrentUserAsync();
        if (portfolio is null || user is null) return NotFound();
        portfolio.ApprovalStatus = approvalStatus;
        portfolio.AdminId = user.EntityId;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Portfolios));
    }

    [HttpGet("requests")]
    public async Task<IActionResult> Requests() => View("~/Views/Admin/requests/index.cshtml", await DbContext.DesignRequests.Include(r => r.Client).Include(r => r.Designer).OrderByDescending(r => r.DateTime).ToListAsync());

    [HttpGet("requests/{requestId:int}")]
    public async Task<IActionResult> ShowRequest(int requestId) => View("~/Views/Admin/requests/show.cshtml", await DbContext.DesignRequests.Include(r => r.Client).Include(r => r.Designer).Include(r => r.Proposals).ThenInclude(p => p.Designer).FirstOrDefaultAsync(r => r.RequestId == requestId));

    [HttpGet("consultations")]
    public async Task<IActionResult> Consultations() => View("~/Views/Admin/consultations/index.cshtml", await DbContext.Consultations.Include(c => c.Client).Include(c => c.Designer).OrderByDescending(c => c.ScheduledAt).ToListAsync());

    [HttpGet("consultations/{id:int}")]
    public async Task<IActionResult> ShowConsultation(int id) => View("~/Views/Admin/consultations/show.cshtml", await DbContext.Consultations.Include(c => c.Client).Include(c => c.Designer).Include(c => c.Messages).FirstOrDefaultAsync(c => c.ConsultationId == id));

    [HttpGet("projects")]
    public async Task<IActionResult> Projects() => View("~/Views/Admin/projects/index.cshtml", await DbContext.Projects.Include(p => p.Proposal).ThenInclude(p => p.DesignRequest).ThenInclude(r => r.Client).Include(p => p.Proposal.Designer).ToListAsync());

    [HttpGet("projects/{id:int}")]
    public async Task<IActionResult> ShowProject(int id) => View("~/Views/Admin/projects/show.cshtml", await DbContext.Projects.Include(p => p.Proposal).ThenInclude(p => p.DesignRequest).ThenInclude(r => r.Client).Include(p => p.Proposal.Designer).Include(p => p.Payments).ThenInclude(pay => pay.Invoice).Include(p => p.Reviews).FirstOrDefaultAsync(p => p.ProjectId == id));

    [HttpGet("payments")]
    public async Task<IActionResult> Payments() => View("~/Views/Admin/payments/index.cshtml", await DbContext.Payments.Include(p => p.Invoice).Include(p => p.Consultation!).ThenInclude(c => c.Client).Include(p => p.Consultation!).ThenInclude(c => c.Designer).Include(p => p.Project!).ThenInclude(pr => pr.Proposal).ThenInclude(pp => pp.DesignRequest).ToListAsync());

    [HttpGet("payments/{id:int}")]
    public async Task<IActionResult> ShowPayment(int id) => View("~/Views/Admin/payments/show.cshtml", await DbContext.Payments.Include(p => p.Invoice).Include(p => p.Consultation!).ThenInclude(c => c.Client).Include(p => p.Consultation!).ThenInclude(c => c.Designer).Include(p => p.Project!).ThenInclude(pr => pr.Proposal).ThenInclude(pp => pp.DesignRequest).FirstOrDefaultAsync(p => p.PaymentId == id));

    [HttpGet("payments/{id:int}/invoice")]
    public async Task<IActionResult> PrintInvoice(int id) => View("~/Views/Admin/payments/invoice.cshtml", await DbContext.Payments.Include(p => p.Invoice).Include(p => p.Consultation!).ThenInclude(c => c.Client).Include(p => p.Consultation!).ThenInclude(c => c.Designer).Include(p => p.Project!).ThenInclude(pr => pr.Proposal).ThenInclude(pp => pp.DesignRequest).FirstOrDefaultAsync(p => p.PaymentId == id));

    [HttpGet("clients")]
    public async Task<IActionResult> Clients() => View("~/Views/Admin/clients/index.cshtml", await DbContext.Clients.Include(c => c.DesignRequests).Include(c => c.Consultations).Include(c => c.Reviews).OrderByDescending(c => c.RegisteredAt).ToListAsync());

    [HttpGet("clients/{id:int}")]
    public async Task<IActionResult> ShowClient(int id) => View("~/Views/Admin/clients/show.cshtml", await DbContext.Clients.Include(c => c.DesignRequests).ThenInclude(r => r.Designer).Include(c => c.Consultations).ThenInclude(cn => cn.Designer).Include(c => c.Reviews).ThenInclude(r => r.Designer).FirstOrDefaultAsync(c => c.ClientId == id));

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews() => View("~/Views/Admin/reviews/index.cshtml", await DbContext.Reviews.Include(r => r.Client).Include(r => r.Designer).Include(r => r.Project).Include(r => r.Consultation).OrderByDescending(r => r.ReviewDate).ToListAsync());
}
