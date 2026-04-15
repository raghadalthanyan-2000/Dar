using System.ComponentModel.DataAnnotations;

namespace dar_system.Models.Shared;

public class StatusUpdateViewModel
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
