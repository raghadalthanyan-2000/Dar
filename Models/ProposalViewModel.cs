using System.ComponentModel.DataAnnotations;

namespace dar_system.ViewModels.Designer;

public class ProposalViewModel
{
    [Required]
    public int RequestId { get; set; }

    [Range(1, 999999999)]
    public decimal Cost { get; set; }

    [Range(1, 3650)]
    public int EstimatedDeliveryTime { get; set; }

    [Required, StringLength(2000, MinimumLength = 20)]
    public string ScopeOfWork { get; set; } = string.Empty;
}
