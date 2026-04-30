using dar_system.Data;
using dar_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer/portfolio")]
public class DesignerPortfolioController(DarDbContext dbContext, IWebHostEnvironment environment) : AppController(dbContext)
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var items = await DbContext.Portfolios.Where(p => p.DesignerId == designer.DesignerId).OrderByDescending(p => p.PortfolioId).ToListAsync();
        return View("~/Views/Designer/portfolio/index.cshtml", items);
    }

    [HttpPost("")]
    public async Task<IActionResult> Store(string title, string description, IFormFile? fileUrl)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        string? relativePath = null;
        if (fileUrl is not null && fileUrl.Length > 0)
        {
            var directory = Path.Combine(environment.WebRootPath, "uploads", "portfolio");
            Directory.CreateDirectory(directory);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(fileUrl.FileName)}";
            var filePath = Path.Combine(directory, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await fileUrl.CopyToAsync(stream);
            relativePath = $"/uploads/portfolio/{fileName}";
        }

        DbContext.Portfolios.Add(new Portfolio { DesignerId = designer.DesignerId, Title = title, Description = description, FileUrl = relativePath, ApprovalStatus = "pending" });
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, string title, string description, IFormFile? fileUrl)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var portfolio = await DbContext.Portfolios.FirstOrDefaultAsync(p => p.PortfolioId == id && p.DesignerId == designer.DesignerId);
        if (portfolio is null) return NotFound();
        portfolio.Title = title;
        portfolio.Description = description;
        portfolio.ApprovalStatus = "pending";
        if (fileUrl is not null && fileUrl.Length > 0)
        {
            var directory = Path.Combine(environment.WebRootPath, "uploads", "portfolio");
            Directory.CreateDirectory(directory);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(fileUrl.FileName)}";
            var filePath = Path.Combine(directory, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await fileUrl.CopyToAsync(stream);
            portfolio.FileUrl = $"/uploads/portfolio/{fileName}";
        }
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpDelete("{id:int}")]
    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Destroy(int id)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var portfolio = await DbContext.Portfolios.FirstOrDefaultAsync(p => p.PortfolioId == id && p.DesignerId == designer.DesignerId);
        if (portfolio is null) return NotFound();
        DbContext.Portfolios.Remove(portfolio);
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
