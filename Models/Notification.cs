using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("notification_id")]
    public int NotificationId { get; set; }

    [Column("receiver_id")]
    public int ReceiverId { get; set; }

    [Required, StringLength(255)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("date_time")]
    public DateTime DateTime { get; set; }
}
