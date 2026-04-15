using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("project")]
public class Project
{
    [Key]
    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("proposal_id")]
    public int ProposalId { get; set; }

    [Required, StringLength(30)]
    [Column("project_status")]
    public string ProjectStatus { get; set; } = "pending";

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    public Proposal Proposal { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
