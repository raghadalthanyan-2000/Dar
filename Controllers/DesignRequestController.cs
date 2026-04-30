using dar_system.Data;
using dar_system.DTOs;
using dar_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize]
[Route("client")]
public class DesignRequestController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("create-request/{designerId:int}")]
    public async Task<IActionResult> CreateRequest(int designerId)
    {
        var client = await GetCurrentClientAsync();
        var designer = await DbContext.Designers.FindAsync(designerId);
        if (client is null || designer is null) return NotFound();

        return View("~/Views/DesignRequests/Create.cshtml", new CreateDesignRequestDto { ClientId = client.ClientId, DesignerId = designerId });
    }

    [HttpPost("store-request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StoreRequest(CreateDesignRequestDto model)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();
        if (model.BudgetMax <= model.BudgetMin) ModelState.AddModelError(nameof(model.BudgetMax), "Budget max must be greater than budget min.");
        if (!ModelState.IsValid) return View("~/Views/DesignRequests/Create.cshtml", model);

        var entity = new DesignRequest
        {
            ClientId = client.ClientId,
            DesignerId = model.DesignerId,
            ProjectTitle = model.ProjectTitle,
            ProjectDescription = model.ProjectDescription,
            ProjectType = model.ProjectType,
            BudgetMin = model.BudgetMin,
            BudgetMax = model.BudgetMax,
            Status = "pending",
            DateTime = DateTime.UtcNow
        };

        DbContext.DesignRequests.Add(entity);
        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Design request submitted successfully.";
        return RedirectToAction("DesignerProfile", "Home", new { id = model.DesignerId });
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> MyRequests(string? status, DateOnly? fromDate, DateOnly? toDate, string? search)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();

        var query = DbContext.DesignRequests
            .Include(r => r.Designer)
            .Include(r => r.Proposals)
            .ThenInclude(p => p.Designer)
            .Where(r => r.ClientId == client.ClientId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();
            query = query.Where(r =>
                r.ProjectTitle.Contains(searchValue) ||
                (r.Designer.FirstName + " " + r.Designer.LastName).Contains(searchValue) ||
                r.Proposals.Any(p => (p.Designer.FirstName + " " + p.Designer.LastName).Contains(searchValue)));
        }

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);
        if (fromDate.HasValue) query = query.Where(r => DateOnly.FromDateTime(r.DateTime) >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(r => DateOnly.FromDateTime(r.DateTime) <= toDate.Value);

        ViewData["Search"] = search;
        return View("~/Views/Client/myRequests.cshtml", await query.OrderByDescending(r => r.DateTime).ToListAsync());
    }

    [HttpPost("accept-proposal/{proposalId:int}")]
    public async Task<IActionResult> AcceptProposal(int proposalId)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();
        var proposal = await DbContext.Proposals.Include(p => p.DesignRequest).FirstOrDefaultAsync(p => p.ProposalId == proposalId);
        if (proposal is null || proposal.DesignRequest.ClientId != client.ClientId) return Forbid();
        if (proposal.ProposalStatus != "pending") return BadRequest();

        proposal.ProposalStatus = "accepted";
        proposal.DesignRequest.Status = "accepted";
        var others = await DbContext.Proposals.Where(p => p.RequestId == proposal.RequestId && p.ProposalId != proposal.ProposalId).ToListAsync();
        foreach (var item in others) item.ProposalStatus = "rejected";
        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Proposal accepted successfully.";
        return RedirectToAction(nameof(MyRequests));
    }

    [HttpPost("reject-proposal/{proposalId:int}")]
    public async Task<IActionResult> RejectProposal(int proposalId)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();
        var proposal = await DbContext.Proposals.Include(p => p.DesignRequest).FirstOrDefaultAsync(p => p.ProposalId == proposalId);
        if (proposal is null || proposal.DesignRequest.ClientId != client.ClientId) return Forbid();
        proposal.ProposalStatus = "rejected";
        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Proposal rejected successfully.";
        return RedirectToAction(nameof(MyRequests));
    }

    [HttpPost("delete-request/{requestId:int}")]
    [HttpDelete("delete-request/{requestId:int}")]
    public async Task<IActionResult> DeleteRequest(int requestId)
    {
        var client = await GetCurrentClientAsync();
        if (client is null) return Challenge();
        var request = await DbContext.DesignRequests.Include(r => r.Proposals).FirstOrDefaultAsync(r => r.RequestId == requestId);
        if (request is null || request.ClientId != client.ClientId) return Forbid();
        DbContext.Proposals.RemoveRange(request.Proposals);
        DbContext.DesignRequests.Remove(request);
        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Request deleted successfully.";
        return RedirectToAction(nameof(MyRequests));
    }
}
