namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class CreateCheckoutSessionResponse
{
    public string CheckoutUrl { get; init; } = string.Empty;

    public string PaymentReference { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;
}
