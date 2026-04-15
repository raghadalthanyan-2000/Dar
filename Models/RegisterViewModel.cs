using System.ComponentModel.DataAnnotations;
using dar_system.Validation;

namespace dar_system.Models.Auth;

public class RegisterViewModel
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, Range(1900, 2100)]
    public int YearOfBirth { get; set; }

    [Required, ValidPhone]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string UserType { get; set; } = "client";

    [StringLength(100)]
    public string? Specialty { get; set; }

    [Range(0, 50)]
    public int? ExperienceYears { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}
