using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("portfolios")]
public class Portfolio
{
    [Key]
    [Column("portfolio_id")]
    public int PortfolioId { get; set; }

    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Required, StringLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    [Column("description")]
    public string? Description { get; set; }

    [StringLength(500)]
    [Column("file_url")]
    public string? FileUrl { get; set; }

    [StringLength(30)]
    [Column("approval_status")]
    public string? ApprovalStatus { get; set; }

    [Column("admin_id")]
    public int? AdminId { get; set; }

    public Administrator? Administrator { get; set; }
    public Designer Designer { get; set; } = null!;
}
