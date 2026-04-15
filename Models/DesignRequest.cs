using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("design_requests")]
public class DesignRequest
{
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Required, StringLength(255)]
    [Column("project_title")]
    public string ProjectTitle { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    [Column("project_description")]
    public string ProjectDescription { get; set; } = string.Empty;

    [Column("images")]
    public string? Images { get; set; }

    [Required, StringLength(100)]
    [Column("project_type")]
    public string ProjectType { get; set; } = string.Empty;

    [Column("budget_min", TypeName = "decimal(18,2)")]
    public decimal BudgetMin { get; set; }

    [Column("budget_max", TypeName = "decimal(18,2)")]
    public decimal BudgetMax { get; set; }

    [Required, StringLength(30)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("date_time")]
    public DateTime DateTime { get; set; }

    public Client Client { get; set; } = null!;
    public Designer Designer { get; set; } = null!;
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
}
