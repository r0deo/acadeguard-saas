using AcademicCompliance.Domain.Enums;

namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class PaymentWebhookResult
{
    public bool IsPaymentSuccessful { get; init; }

    public Guid OrganizationId { get; init; }

    public PaymentProvider Provider { get; init; }

    public string PaymentReference { get; init; } = string.Empty;

    public string TransactionId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;
}
