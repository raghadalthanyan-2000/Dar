using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("designer")]
public class Designer
{
    [Key]
    [Column("designer_id")]
    public int DesignerId { get; set; }

    [Required, StringLength(50)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("year_of_birth")]
    public int? YearOfBirth { get; set; }

    [StringLength(30)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Required, EmailAddress, StringLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("password")]
    public string? Password { get; set; }

    [StringLength(2000)]
    [Column("bio")]
    public string? Bio { get; set; }

    [Column("experience_years")]
    public int? ExperienceYears { get; set; }

    [StringLength(100)]
    [Column("specialty")]
    public string? Specialty { get; set; }

    [StringLength(30)]
    [Column("verification_status")]
    public string? VerificationStatus { get; set; }

    [Column("rating_avg", TypeName = "decimal(4,2)")]
    public decimal? RatingAverage { get; set; }

    [Column("registered_at")]
    public DateTime? RegisteredAt { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DesignRequest> DesignRequests { get; set; } = new List<DesignRequest>();
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
