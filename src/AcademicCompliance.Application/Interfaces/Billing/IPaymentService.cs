using AcademicCompliance.Application.DTOs.Billing;

namespace AcademicCompliance.Application.Interfaces.Billing;

public interface IPaymentService
{
    Task<CreateCheckoutResponse> CreateCheckoutAsync(Guid organizationId);

    Task<CheckoutReturnResponse> CompleteCheckoutReturnAsync(string paymentReference);

    Task HandleWebhookAsync(string payload, string signature);

    Task<List<PaymentResponse>> GetByOrganizationAsync(Guid organizationId);
}
