using dar_system.Data;
using dar_system.Models.Shared;
using dar_system.ViewModels.Designer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer/consultations")]
public class DesignerConsultationController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? status, DateOnly? fromDate, DateOnly? toDate, string? search)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var query = DbContext.Consultations.Include(c => c.Client).Where(c => c.DesignerId == designer.DesignerId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.ConsultationStatus == status);
        if (fromDate.HasValue) query = query.Where(c => DateOnly.FromDateTime(c.ScheduledAt) >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(c => DateOnly.FromDateTime(c.ScheduledAt) <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.Topic.Contains(search) || c.Description.Contains(search) || c.Client.FirstName.Contains(search) || c.Client.LastName.Contains(search));
        return View("~/Views/Designer/consultations/index.cshtml", await query.OrderByDescending(c => c.ScheduledAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var consultation = await DbContext.Consultations.Include(c => c.Client).FirstOrDefaultAsync(c => c.ConsultationId == id && c.DesignerId == designer.DesignerId);
        if (consultation is null) return NotFound();
        var history = await DbContext.Consultations.Where(c => c.ClientId == consultation.ClientId && c.ConsultationId != id && c.DesignerId == designer.DesignerId).OrderByDescending(c => c.ScheduledAt).Take(5).ToListAsync();
        return View("~/Views/Designer/consultations/show.cshtml", new { Consultation = consultation, ClientHistory = history });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        return View("~/Views/Designer/consultations/edit.cshtml", consultation);
    }

    [HttpPost("{id:int}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ConsultationUpdateViewModel model)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        consultation.Topic = model.Topic;
        consultation.Description = model.Description ?? string.Empty;
        consultation.FixedPrice = model.FixedPrice;
        consultation.ScheduledAt = model.ScheduledDate.ToDateTime(model.ScheduledTime);
        await DbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Consultation updated successfully.";
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPut("{id:int}/approve")]
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        consultation.ConsultationStatus = "approved";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPut("{id:int}/reject")]
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, ReasonViewModel model)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        consultation.ConsultationStatus = "rejected";
        consultation.Description = $"{consultation.Description}\n\nRejection Reason: {model.Reason}";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPut("{id:int}/complete")]
    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        consultation.ConsultationStatus = "completed";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPut("{id:int}/cancel")]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, ReasonViewModel model)
    {
        var consultation = await DbContext.Consultations.FindAsync(id);
        if (consultation is null) return NotFound();
        consultation.ConsultationStatus = "cancelled";
        consultation.Description = $"{consultation.Description}\n\nCancellation Reason: {model.Reason}";
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpGet("calendar")]
    public async Task<IActionResult> Calendar()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var consultations = await DbContext.Consultations.Include(c => c.Client).Where(c => c.DesignerId == designer.DesignerId && (c.ConsultationStatus == "approved" || c.ConsultationStatus == "scheduled" || c.ConsultationStatus == "completed")).OrderBy(c => c.ScheduledAt).ToListAsync();
        return View("~/Views/Designer/consultations/calendar.cshtml", consultations);
    }

    [HttpGet("calendar-events")]
    public async Task<IActionResult> GetCalendarEvents()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var consultations = await DbContext.Consultations.Include(c => c.Client).Where(c => c.DesignerId == designer.DesignerId && (c.ConsultationStatus == "approved" || c.ConsultationStatus == "scheduled" || c.ConsultationStatus == "completed")).ToListAsync();
        var events = consultations.Select(c => new { id = c.ConsultationId, title = $"{c.Topic} - {c.Client.FirstName}", start = c.ScheduledAt, end = c.ScheduledAt.AddHours(1), color = c.ConsultationStatus }).ToList();
        return Json(events);
    }
}
