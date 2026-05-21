namespace AcademicCompliance.Application.DTOs.Organizations;

public sealed class OrganizationListItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string OfficialEmail { get; init; } = string.Empty;

    public string? ContactPersonName { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}
