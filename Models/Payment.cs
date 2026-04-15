using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("payment")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    public int PaymentId { get; set; }

    [Column("project_id")]
    public int? ProjectId { get; set; }

    [Column("consultation_id")]
    public int? ConsultationId { get; set; }

    [Column("amount", TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required, StringLength(50)]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Column("payment_status")]
    public string PaymentStatus { get; set; } = "pending";

    [Column("payment_date")]
    public DateTime PaymentDate { get; set; }

    public Consultation? Consultation { get; set; }
    public Invoice? Invoice { get; set; }
    public Project? Project { get; set; }
}
