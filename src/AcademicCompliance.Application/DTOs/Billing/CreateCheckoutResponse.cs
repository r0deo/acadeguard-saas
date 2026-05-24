namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class CreateCheckoutResponse
{
    public string CheckoutUrl { get; init; } = string.Empty;

    public string PaymentReference { get; init; } = string.Empty;
}
