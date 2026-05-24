namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class CreateCheckoutSessionRequest
{
    public Guid OrganizationId { get; init; }

    public string OrganizationName { get; init; } = string.Empty;

    public string OfficialEmail { get; init; } = string.Empty;
}
