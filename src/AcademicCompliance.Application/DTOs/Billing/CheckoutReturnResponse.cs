namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class CheckoutReturnResponse
{
    public Guid OrganizationId { get; init; }

    public string PaymentReference { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime? SubscriptionEndDate { get; init; }

    public string Message { get; init; } = string.Empty;
}
