using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace dar_system.Validation;

public partial class ValidPhoneAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^\+?[0-9]{7,15}$")]
    private static partial Regex PhoneRegex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        return value is string phone && PhoneRegex().IsMatch(phone)
            ? ValidationResult.Success
            : new ValidationResult("Phone number format is invalid.");
    }
}
