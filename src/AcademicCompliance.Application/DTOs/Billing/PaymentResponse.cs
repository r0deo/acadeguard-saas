namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class PaymentResponse
{
    public Guid Id { get; init; }

    public string Provider { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public DateTime? PaidAt { get; init; }
}
