using dar_system.Data;
using dar_system.Models;
using dar_system.ViewModels.Designer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer/requests")]
public class DesignerRequestController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? type, decimal? budgetMin, decimal? budgetMax, string? search, string? sort, string? direction)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var query = DbContext.DesignRequests.Include(r => r.Client).AsQueryable();
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(r => r.ProjectType == type);
        if (budgetMin.HasValue) query = query.Where(r => r.BudgetMax >= budgetMin.Value);
        if (budgetMax.HasValue) query = query.Where(r => r.BudgetMin <= budgetMax.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(r => r.ProjectTitle.Contains(search) || r.ProjectDescription.Contains(search));
        query = (sort, direction) switch
        {
            ("project_title", "asc") => query.OrderBy(r => r.ProjectTitle),
            ("project_title", _) => query.OrderByDescending(r => r.ProjectTitle),
            (_, "asc") => query.OrderBy(r => r.DateTime),
            _ => query.OrderByDescending(r => r.DateTime)
        };
        return View("~/Views/Designer/requests/index.cshtml", new
        {
            Requests = await query.ToListAsync(),
            TotalPending = await DbContext.DesignRequests.CountAsync(r => r.Status == "pending"),
            MyProposals = await DbContext.Proposals.CountAsync(p => p.DesignerId == designer.DesignerId),
            TotalWithProposals = await DbContext.DesignRequests.CountAsync(r => r.Proposals.Any()),
            AcceptedCount = await DbContext.Proposals.CountAsync(p => p.DesignerId == designer.DesignerId && p.ProposalStatus == "accepted")
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var request = await DbContext.DesignRequests.Include(r => r.Client).Include(r => r.Proposals).ThenInclude(p => p.Designer).FirstOrDefaultAsync(r => r.RequestId == id);
        if (request is null) return NotFound();
        return View("~/Views/Designer/requests/show.cshtml", new
        {
            DesignRequest = request,
            MyProposals = request.Proposals.Where(p => p.DesignerId == designer.DesignerId),
            AllProposals = request.Proposals
        });
    }

    [HttpGet("create-proposal/{requestId:int}")]
    public async Task<IActionResult> CreateProposal(int requestId)
    {
        var request = await DbContext.DesignRequests.FindAsync(requestId);
        return request is null ? NotFound() : View("~/Views/Designer/requests/createProposal.cshtml", request);
    }

    [HttpPost("store-proposal")]
    public async Task<IActionResult> StoreProposal(ProposalViewModel model)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var request = await DbContext.DesignRequests.FindAsync(model.RequestId);
        if (request is null) return NotFound();

        DbContext.Proposals.Add(new Proposal
        {
            RequestId = model.RequestId,
            DesignerId = designer.DesignerId,
            Cost = model.Cost,
            EstimatedDeliveryTime = model.EstimatedDeliveryTime,
            ScopeOfWork = model.ScopeOfWork,
            ProposalStatus = "pending"
        });
        request.Status = "with proposal";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id = model.RequestId });
    }

    [HttpGet("edit-proposal/{proposalId:int}")]
    public async Task<IActionResult> EditProposal(int proposalId)
    {
        var proposal = await DbContext.Proposals.Include(p => p.DesignRequest).FirstOrDefaultAsync(p => p.ProposalId == proposalId);
        return proposal is null ? NotFound() : View("~/Views/Designer/requests/editProposal.cshtml", proposal);
    }

    [HttpPost("update-proposal/{proposalId:int}")]
    [HttpPut("update-proposal/{proposalId:int}")]
    public async Task<IActionResult> UpdateProposal(int proposalId, ProposalViewModel model)
    {
        var proposal = await DbContext.Proposals.FindAsync(proposalId);
        if (proposal is null) return NotFound();
        proposal.Cost = model.Cost;
        proposal.EstimatedDeliveryTime = model.EstimatedDeliveryTime;
        proposal.ScopeOfWork = model.ScopeOfWork;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id = proposal.RequestId });
    }

    [HttpDelete("withdraw-proposal/{proposalId:int}")]
    [HttpPost("withdraw-proposal/{proposalId:int}")]
    public async Task<IActionResult> WithdrawProposal(int proposalId)
    {
        var proposal = await DbContext.Proposals.FindAsync(proposalId);
        if (proposal is null) return NotFound();
        var requestId = proposal.RequestId;
        DbContext.Proposals.Remove(proposal);
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id = requestId });
    }

    [HttpGet("my-proposals/list")]
    public async Task<IActionResult> MyProposals(string? status)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var query = DbContext.Proposals.Include(p => p.DesignRequest).ThenInclude(r => r.Client).Where(p => p.DesignerId == designer.DesignerId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.ProposalStatus == status);
        return View("~/Views/Designer/requests/myProposals.cshtml", await query.OrderByDescending(p => p.ProposalId).ToListAsync());
    }

    [HttpGet("create-project/{proposalId:int}")]
    public async Task<IActionResult> CreateProject(int proposalId)
    {
        var proposal = await DbContext.Proposals.Include(p => p.DesignRequest).ThenInclude(r => r.Client).FirstOrDefaultAsync(p => p.ProposalId == proposalId);
        return proposal is null ? NotFound() : View("~/Views/Designer/requests/createProject.cshtml", proposal);
    }

    [HttpPost("store-project/{proposalId:int}")]
    public async Task<IActionResult> StoreProject(int proposalId, ProjectCreateViewModel model)
    {
        var proposal = await DbContext.Proposals.Include(p => p.DesignRequest).FirstOrDefaultAsync(p => p.ProposalId == proposalId);
        if (proposal is null) return NotFound();
        DbContext.Projects.Add(new Project { ProposalId = proposalId, ProjectStatus = "in_progress", StartDate = model.StartDate, EndDate = model.EstimatedEndDate });
        proposal.DesignRequest.Status = "in_progress";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id = proposal.RequestId });
    }
}
