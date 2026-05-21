using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Organizations;

public sealed class CreateOrganizationRequest
{
    [Required]
    [StringLength(250)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string OfficialEmail { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? LogoUrl { get; init; }

    [StringLength(200)]
    public string? ContactPersonName { get; init; }

    [EmailAddress]
    [StringLength(250)]
    public string? ContactPersonEmail { get; init; }

    [StringLength(50)]
    public string? PhoneNumber { get; init; }

    [StringLength(500)]
    public string? Address { get; init; }
}
