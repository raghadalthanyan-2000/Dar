using System.ComponentModel.DataAnnotations;

namespace dar_system.Models.Shared;

public class ReasonViewModel
{
    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
