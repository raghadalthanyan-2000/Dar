using System.ComponentModel.DataAnnotations;

namespace dar_system.ViewModels.Designer;

public class ProjectCreateViewModel
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EstimatedEndDate { get; set; }

    [StringLength(1000)]
    public string? ProjectNotes { get; set; }
}
