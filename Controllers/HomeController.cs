using System.Diagnostics;
using dar_system.Data;
using dar_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dar_system.Controllers;

public class HomeController(DarDbContext dbContext) : AppController(dbContext)
{
    [HttpGet("")]
    [HttpGet("home")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("find-designers")]
    public async Task<IActionResult> FindDesigners(string? search, List<string>? specialties, string? experience, decimal? rating, string? sort, int page = 1, int pageSize = 12)
    {
        var query = DbContext.Designers.AsNoTracking().Include(d => d.Reviews).Where(d => d.VerificationStatus == "verified");

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.FirstName.Contains(search) || d.LastName.Contains(search) || (d.Specialty ?? string.Empty).Contains(search) || (d.Bio ?? string.Empty).Contains(search));
        }

        if (specialties is { Count: > 0 })
        {
            query = query.Where(d => d.Specialty != null && specialties.Contains(d.Specialty));
        }

        query = experience switch
        {
            "junior" => query.Where(d => d.ExperienceYears >= 1 && d.ExperienceYears <= 3),
            "mid" => query.Where(d => d.ExperienceYears >= 4 && d.ExperienceYears <= 7),
            "senior" => query.Where(d => d.ExperienceYears >= 8),
            "expert" => query.Where(d => d.ExperienceYears >= 15),
            _ => query
        };

        if (rating.HasValue)
        {
            query = query.Where(d => (d.RatingAverage ?? 0) >= rating.Value);
        }

        query = sort switch
        {
            "rating_asc" => query.OrderBy(d => d.RatingAverage),
            "experience_desc" => query.OrderByDescending(d => d.ExperienceYears),
            "experience_asc" => query.OrderBy(d => d.ExperienceYears),
            "name_asc" => query.OrderBy(d => d.FirstName).ThenBy(d => d.LastName),
            "newest" => query.OrderByDescending(d => d.RegisteredAt),
            "oldest" => query.OrderBy(d => d.RegisteredAt),
            _ => query.OrderByDescending(d => d.RatingAverage)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var allSpecialties = await DbContext.Designers.AsNoTracking().Where(d => d.VerificationStatus == "verified" && d.Specialty != null).Select(d => d.Specialty!).Distinct().OrderBy(x => x).ToListAsync();
        var verified = DbContext.Designers.AsNoTracking().Where(d => d.VerificationStatus == "verified");
        var model = new
        {
            Designers = items.Select(d => new
            {
                d.DesignerId,
                d.FullName,
                d.Specialty,
                d.Bio,
                ExperienceYears = d.ExperienceYears ?? 0,
                RatingAverage = d.RatingAverage ?? 0,
                ReviewCount = d.Reviews.Count
            }),
            Filters = new { search, specialties, experience, rating, sort, page, pageSize },
            Specialties = allSpecialties,
            Stats = new
            {
                TotalDesigners = await verified.CountAsync(),
                AverageRating = Math.Round(await verified.Select(d => d.RatingAverage ?? 0m).DefaultIfEmpty().AverageAsync(), 1),
                TotalSpecialties = allSpecialties.Count,
                TopDesigners = await verified.CountAsync(d => (d.RatingAverage ?? 0) >= 4.5m)
            },
            Pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
        };

        return View("~/Views/Home/FindDesigners.cshtml", model);
    }

    [HttpGet("designers/{id:int}")]
    public async Task<IActionResult> DesignerProfile(int id)
    {
        var designer = await DbContext.Designers.AsNoTracking()
            .Include(d => d.Reviews).ThenInclude(r => r.Client)
            .Include(d => d.Portfolios.Where(p => p.ApprovalStatus == "approved"))
            .Include(d => d.DesignRequests)
            .FirstOrDefaultAsync(d => d.DesignerId == id && d.VerificationStatus == "verified");

        if (designer is null)
        {
            return NotFound();
        }

        var model = new
        {
            designer.DesignerId,
            designer.FullName,
            designer.Specialty,
            designer.Bio,
            ExperienceYears = designer.ExperienceYears ?? 0,
            RatingAverage = designer.RatingAverage ?? 0,
            CompletedProjects = designer.DesignRequests.Count(r => r.Status == "completed"),
            Portfolios = designer.Portfolios,
            Reviews = designer.Reviews.OrderByDescending(r => r.ReviewDate).Select(r => new { ClientName = r.Client.FullName, r.Rating, r.Comment, r.ReviewDate })
        };

        return View("~/Views/Home/DesignerProfile.cshtml", model);
    }

    [HttpPost("filter-designers")]
    public async Task<IActionResult> FilterDesigners(string? search, List<string>? specialties, string? experience, decimal? rating, string? sort)
    {
        return await FindDesigners(search, specialties, experience, rating, sort);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
