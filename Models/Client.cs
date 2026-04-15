using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("client")]
public class Client
{
    [Key]
    [Column("client_id")]
    public int ClientId { get; set; }

    [Required, StringLength(50)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("year_of_birth")]
    public int YearOfBirth { get; set; }

    [Required, StringLength(30)]
    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("password")]
    public string? Password { get; set; }

    [Column("registered_at")]
    public DateTime? RegisteredAt { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DesignRequest> DesignRequests { get; set; } = new List<DesignRequest>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
