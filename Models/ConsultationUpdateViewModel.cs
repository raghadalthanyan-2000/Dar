using System.ComponentModel.DataAnnotations;

namespace dar_system.ViewModels.Designer;

public class ConsultationUpdateViewModel
{
    [Required]
    public DateOnly ScheduledDate { get; set; }

    [Required]
    public TimeOnly ScheduledTime { get; set; }

    [Required, StringLength(255)]
    public string Topic { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 999999999)]
    public decimal FixedPrice { get; set; }
}
