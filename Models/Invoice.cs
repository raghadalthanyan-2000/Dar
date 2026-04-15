using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("invoice")]
public class Invoice
{
    [Key]
    [Column("invoice_id")]
    public int InvoiceId { get; set; }

    [Column("payment_id")]
    public int PaymentId { get; set; }

    [Column("service_fee", TypeName = "decimal(18,2)")]
    public decimal ServiceFee { get; set; }

    [Column("total_amount", TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column("generated_date")]
    public DateTime GeneratedDate { get; set; }

    public Payment Payment { get; set; } = null!;
}
