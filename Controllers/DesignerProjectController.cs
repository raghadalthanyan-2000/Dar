using dar_system.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer/projects")]
public class DesignerProjectController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? status, DateOnly? fromDate, DateOnly? toDate, string? search)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var query = DbContext.Projects.Include(p => p.Proposal).ThenInclude(p => p.DesignRequest).ThenInclude(r => r.Client).Include(p => p.Payments).Where(p => p.Proposal.DesignerId == designer.DesignerId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.ProjectStatus == status);
        if (fromDate.HasValue) query = query.Where(p => p.StartDate.HasValue && p.StartDate.Value >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.StartDate.HasValue && p.StartDate.Value <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Proposal.DesignRequest.ProjectTitle.Contains(search) || p.Proposal.DesignRequest.Client.FirstName.Contains(search) || p.Proposal.DesignRequest.Client.LastName.Contains(search));
        return View("~/Views/Designer/projects/index.cshtml", await query.OrderByDescending(p => p.StartDate).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var project = await DbContext.Projects.Include(p => p.Proposal).ThenInclude(p => p.DesignRequest).ThenInclude(r => r.Client).Include(p => p.Payments).ThenInclude(p => p.Invoice).Include(p => p.Reviews).FirstOrDefaultAsync(p => p.ProjectId == id && p.Proposal.DesignerId == designer.DesignerId);
        return project is null ? NotFound() : View("~/Views/Designer/projects/show.cshtml", project);
    }

    [HttpPost("{id:int}/status")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, string projectStatus)
    {
        var project = await DbContext.Projects.FindAsync(id);
        if (project is null) return NotFound();
        project.ProjectStatus = projectStatus;
        if (projectStatus == "completed") project.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id });
    }
}
