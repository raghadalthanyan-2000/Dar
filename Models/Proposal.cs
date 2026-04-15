using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("proposals")]
public class Proposal
{
    [Key]
    [Column("proposal_id")]
    public int ProposalId { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Column("cost", TypeName = "decimal(18,2)")]
    public decimal Cost { get; set; }

    [Column("estimated_delivery_time")]
    public int EstimatedDeliveryTime { get; set; }

    [Required, StringLength(4000)]
    [Column("scope_of_work")]
    public string ScopeOfWork { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Column("proposal_status")]
    public string ProposalStatus { get; set; } = "pending";

    public DesignRequest DesignRequest { get; set; } = null!;
    public Designer Designer { get; set; } = null!;
    public Project? Project { get; set; }
}
