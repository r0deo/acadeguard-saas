using AcademicCompliance.Application.DTOs.Billing;

namespace AcademicCompliance.Application.Interfaces.Billing;

public interface IPaymentProvider
{
    Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request);

    Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentReference);

    Task HandleWebhookAsync(string payload, string signature);

    Task<PaymentWebhookResult?> ParseWebhookAsync(string payload, string signature);
}
