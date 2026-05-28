namespace AcademicCompliance.Application.DTOs.Auth;

public sealed class RegisterOrganizationResponse
{
    public Guid OrganizationId { get; init; }

    public Guid UserId { get; init; }

    public string CheckoutUrl { get; init; } = string.Empty;

    public string PaymentReference { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
