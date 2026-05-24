using AcademicCompliance.Application.DTOs.Billing;

namespace AcademicCompliance.Application.Interfaces.Billing;

public interface ISubscriptionService
{
    Task<SubscriptionStatusResponse> GetStatusAsync(Guid organizationId);

    Task<bool> HasActiveSubscriptionAsync(Guid organizationId);

    Task<CreateCheckoutResponse> CreateCheckoutAsync(Guid organizationId);

    Task ActivateSubscriptionAfterPaymentAsync(Guid organizationId, string paymentReference, decimal amount, string currency);

    Task ExpireOldSubscriptionsAsync();
}
