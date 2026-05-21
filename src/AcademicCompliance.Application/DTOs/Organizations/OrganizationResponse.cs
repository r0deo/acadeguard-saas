namespace AcademicCompliance.Application.DTOs.Organizations;

public sealed class OrganizationResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string OfficialEmail { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string? ContactPersonName { get; init; }

    public string? ContactPersonEmail { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Address { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
