using System.ComponentModel.DataAnnotations;

namespace dar_system.Validation;

public class MinimumAgeAttribute(int minimumAge) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not int yearOfBirth)
        {
            return ValidationResult.Success;
        }

        return DateTime.UtcNow.Year - yearOfBirth >= minimumAge
            ? ValidationResult.Success
            : new ValidationResult($"Minimum age is {minimumAge}.");
    }
}
