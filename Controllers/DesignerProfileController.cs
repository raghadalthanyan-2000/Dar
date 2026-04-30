using BCrypt.Net;
using dar_system.Data;
using dar_system.DTOs;
using dar_system.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dar_system.Controllers;

[Authorize(Roles = "designer")]
[Route("designer/profile")]
public class DesignerProfileController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("")]
    public async Task<IActionResult> Edit()
    {
        var designer = await GetCurrentDesignerAsync();
        if (designer is null) return Challenge();
        var model = new
        {
            designer.DesignerId,
            designer.FirstName,
            designer.LastName,
            YearOfBirth = designer.YearOfBirth ?? 0,
            designer.Phone,
            designer.Email,
            designer.Bio,
            designer.Specialty,
            ExperienceYears = designer.ExperienceYears ?? 0
        };
        return View("~/Views/Designer/profile.cshtml", model);
    }

    [HttpPost("")]
    [HttpPut("")]
    public async Task<IActionResult> Update(ClientProfileDto model, string specialty, int experienceYears, string? bio)
    {
        var designer = await GetCurrentDesignerAsync();
        var user = await GetCurrentUserAsync();
        if (designer is null || user is null) return Challenge();
        designer.FirstName = model.FirstName;
        designer.LastName = model.LastName;
        designer.YearOfBirth = model.YearOfBirth;
        designer.Phone = model.Phone;
        designer.Email = model.Email;
        designer.Specialty = specialty;
        designer.ExperienceYears = experienceYears;
        designer.Bio = bio;
        user.FullName = $"{model.FirstName} {model.LastName}".Trim();
        user.Email = model.Email;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Edit));
    }

    [HttpPost("password")]
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(PasswordChangeViewModel model)
    {
        var designer = await GetCurrentDesignerAsync();
        var user = await GetCurrentUserAsync();
        if (designer is null || user is null) return Challenge();
        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password)) return BadRequest();
        var hashed = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.Password = hashed;
        designer.Password = hashed;
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Edit));
    }
}
