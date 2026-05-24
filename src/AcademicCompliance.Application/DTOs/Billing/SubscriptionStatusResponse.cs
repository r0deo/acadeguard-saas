namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class SubscriptionStatusResponse
{
    public Guid OrganizationId { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    public int DaysRemaining { get; init; }

    public bool IsActive { get; init; }
}
