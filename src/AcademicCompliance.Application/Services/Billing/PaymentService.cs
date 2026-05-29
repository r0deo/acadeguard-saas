using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Billing;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Billing;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Billing;

public sealed class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentProvider _paymentProvider;
    private readonly ISubscriptionService _subscriptionService;

    public PaymentService(
        IApplicationDbContext dbContext,
        IPaymentProvider paymentProvider,
        ISubscriptionService subscriptionService)
    {
        _dbContext = dbContext;
        _paymentProvider = paymentProvider;
        _subscriptionService = subscriptionService;
    }

    public Task<CreateCheckoutResponse> CreateCheckoutAsync(Guid organizationId)
    {
        return _subscriptionService.CreateCheckoutAsync(organizationId);
    }

    public async Task<CheckoutReturnResponse> CompleteCheckoutReturnAsync(string paymentReference)
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            throw new ValidationException("Payment reference is required.");
        }

        var normalizedPaymentReference = paymentReference.Trim();
        var subscription = await _dbContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(subscription => subscription.PaymentReference == normalizedPaymentReference);

        if (subscription is null)
        {
            throw new KeyNotFoundException("Checkout session was not found.");
        }

        var verification = await _paymentProvider.VerifyPaymentAsync(normalizedPaymentReference);
        if (!verification.IsSuccessful)
        {
            var pendingStatus = await _subscriptionService.GetStatusAsync(subscription.OrganizationId);

            return new CheckoutReturnResponse
            {
                OrganizationId = subscription.OrganizationId,
                PaymentReference = normalizedPaymentReference,
                Status = pendingStatus.Status,
                IsActive = pendingStatus.IsActive,
                SubscriptionEndDate = pendingStatus.EndDate,
                Message = "Payment has not been completed yet."
            };
        }

        await _subscriptionService.ActivateSubscriptionAfterPaymentAsync(
            subscription.OrganizationId,
            normalizedPaymentReference,
            verification.Amount,
            verification.Currency);

        var activeStatus = await _subscriptionService.GetStatusAsync(subscription.OrganizationId);

        return new CheckoutReturnResponse
        {
            OrganizationId = subscription.OrganizationId,
            PaymentReference = normalizedPaymentReference,
            Status = activeStatus.Status,
            IsActive = activeStatus.IsActive,
            SubscriptionEndDate = activeStatus.EndDate,
            Message = "Payment verified. Subscription is active."
        };
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        var webhookResult = await _paymentProvider.ParseWebhookAsync(payload, signature);

        if (webhookResult is null || !webhookResult.IsPaymentSuccessful)
        {
            return;
        }

        await _subscriptionService.ActivateSubscriptionAfterPaymentAsync(
            webhookResult.OrganizationId,
            webhookResult.PaymentReference,
            webhookResult.Amount,
            webhookResult.Currency);
    }

    public async Task<List<PaymentResponse>> GetByOrganizationAsync(Guid organizationId)
    {
        var organizationExists = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(organization => organization.Id == organizationId);

        if (!organizationExists)
        {
            throw new KeyNotFoundException("Organization was not found.");
        }

        return await _dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrganizationId == organizationId)
            .OrderByDescending(payment => payment.CreatedAt)
            .Select(payment => new PaymentResponse
            {
                Id = payment.Id,
                Provider = payment.Provider.ToString(),
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaidAt = payment.PaidAt
            })
            .ToListAsync();
    }
}
