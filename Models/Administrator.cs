using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dar_system.Models;

[Table("administrators")]
public class Administrator
{
    [Key]
    [Column("admin_id")]
    public int AdminId { get; set; }

    [Required, StringLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}
