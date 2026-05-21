namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public Guid? OrganizationId { get; init; }

    public bool IsActive { get; init; }
}
