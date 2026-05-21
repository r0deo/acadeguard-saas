using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class RegisterOrganizationUserDto : IValidatableObject
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public Guid OrganizationId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OrganizationId == Guid.Empty)
        {
            yield return new ValidationResult(
                "OrganizationId is required.",
                [nameof(OrganizationId)]);
        }
    }
}
