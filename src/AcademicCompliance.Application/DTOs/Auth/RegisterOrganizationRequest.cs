using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class RegisterOrganizationRequest
{
    [Required]
    [StringLength(250)]
    public string OrganizationName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string OfficialEmail { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? LogoUrl { get; init; }

    [Required]
    [StringLength(200)]
    public string ContactPersonName { get; init; } = string.Empty;

    [EmailAddress]
    [StringLength(250)]
    public string? ContactPersonEmail { get; init; }

    [StringLength(50)]
    public string? PhoneNumber { get; init; }

    [StringLength(500)]
    public string? Address { get; init; }

    [Required]
    [StringLength(200)]
    public string AdminFullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(250)]
    public string AdminEmail { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
