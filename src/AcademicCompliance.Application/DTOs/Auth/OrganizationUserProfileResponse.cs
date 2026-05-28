namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class OrganizationUserProfileResponse
{
    public Guid UserId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string? OrganizationName { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public string? SubscriptionStatus { get; init; }

    public DateTime? SubscriptionEndDate { get; init; }
}
