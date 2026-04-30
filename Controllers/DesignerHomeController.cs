using dar_system.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer")]
public class DesignerHomeController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();

        var model = new
        {
            Designer = designer,
            TotalRequests = await DbContext.DesignRequests.CountAsync(r => r.Proposals.Any(p => p.DesignerId == designer.DesignerId)),
            PendingRequests = await DbContext.DesignRequests.CountAsync(r => r.Status == "pending" && !r.Proposals.Any(p => p.DesignerId == designer.DesignerId)),
            AcceptedProposals = await DbContext.Proposals.CountAsync(p => p.DesignerId == designer.DesignerId && p.ProposalStatus == "accepted"),
            CompletedProjects = await DbContext.Projects.CountAsync(p => p.Proposal.DesignerId == designer.DesignerId && p.ProjectStatus == "completed"),
            RecentRequests = await DbContext.DesignRequests.Include(r => r.Client).Where(r => r.Status == "pending").OrderByDescending(r => r.DateTime).Take(5).ToListAsync(),
            MyProposals = await DbContext.Proposals.Include(p => p.DesignRequest).ThenInclude(r => r.Client).Where(p => p.DesignerId == designer.DesignerId).OrderByDescending(p => p.ProposalId).Take(5).ToListAsync(),
            UpcomingConsultations = await DbContext.Consultations.Include(c => c.Client).Where(c => c.DesignerId == designer.DesignerId && (c.ConsultationStatus == "approved" || c.ConsultationStatus == "scheduled") && c.ScheduledAt > DateTime.UtcNow).OrderBy(c => c.ScheduledAt).Take(5).ToListAsync()
        };
        return View("~/Views/Designer/dashboard.cshtml", model);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();

        var total = await DbContext.Proposals.CountAsync(p => p.DesignerId == designer.DesignerId);
        var completed = await DbContext.Projects.CountAsync(p => p.Proposal.DesignerId == designer.DesignerId && p.ProjectStatus == "completed");
        return Json(new
        {
            total_requests = await DbContext.DesignRequests.CountAsync(r => r.Proposals.Any(p => p.DesignerId == designer.DesignerId)),
            pending_requests = await DbContext.DesignRequests.CountAsync(r => r.Status == "pending" && !r.Proposals.Any(p => p.DesignerId == designer.DesignerId)),
            accepted_proposals = await DbContext.Proposals.CountAsync(p => p.DesignerId == designer.DesignerId && p.ProposalStatus == "accepted"),
            completed_projects = completed,
            total_earnings = await DbContext.Proposals.Where(p => p.DesignerId == designer.DesignerId && p.ProposalStatus == "accepted").SumAsync(p => p.Cost),
            completion_rate = total == 0 ? 0 : Math.Round((double)completed / total * 100)
        });
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> MyReviews(int? rating)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var query = DbContext.Reviews.Include(r => r.Client).Include(r => r.Consultation).Where(r => r.DesignerId == designer.DesignerId);
        if (rating.HasValue) query = query.Where(r => r.Rating == rating.Value);
        return View("~/Views/Designer/reviews.cshtml", await query.OrderByDescending(r => r.ReviewDate).ToListAsync());
    }
}
