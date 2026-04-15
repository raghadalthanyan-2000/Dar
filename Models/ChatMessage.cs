using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("chat_messages")]
public class ChatMessage
{
    [Key]
    [Column("message_id")]
    public int MessageId { get; set; }

    [Column("project_id")]
    public int? ProjectId { get; set; }

    [Column("consultation_id")]
    public int? ConsultationId { get; set; }

    [Column("receiver_id")]
    public int ReceiverId { get; set; }

    [Column("sender_id")]
    public int SenderId { get; set; }

    [Required, StringLength(4000)]
    [Column("message_text")]
    public string MessageText { get; set; } = string.Empty;

    [Column("sent_at")]
    public DateTime SentAt { get; set; }

    public Consultation? Consultation { get; set; }
    public Project? Project { get; set; }
}
