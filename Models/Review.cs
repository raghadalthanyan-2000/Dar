using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("reviews")]
public class Review
{
    [Key]
    [Column("review_id")]
    public int ReviewId { get; set; }

    [Column("project_id")]
    public int? ProjectId { get; set; }

    [Column("consultation_id")]
    public int? ConsultationId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Range(1, 5)]
    [Column("rating")]
    public int Rating { get; set; }

    [Required, StringLength(500)]
    [Column("comment")]
    public string Comment { get; set; } = string.Empty;

    [Column("review_date")]
    public DateTime ReviewDate { get; set; }

    public Client Client { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public Designer Designer { get; set; } = null!;
    public Project? Project { get; set; }
}
