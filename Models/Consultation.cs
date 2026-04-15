using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("consultations")]
public class Consultation
{
    [Key]
    [Column("consultation_id")]
    public int ConsultationId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Required, StringLength(255)]
    [Column("topic")]
    public string Topic { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("fixed_price", TypeName = "decimal(18,2)")]
    public decimal FixedPrice { get; set; }

    [Required, StringLength(30)]
    [Column("consultation_status")]
    public string ConsultationStatus { get; set; } = "pending";

    [Column("scheduled_at")]
    public DateTime ScheduledAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Designer Designer { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
