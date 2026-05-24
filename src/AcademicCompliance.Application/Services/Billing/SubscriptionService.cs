using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Billing;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Entities;
using AcademicCompliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Billing;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentProvider _paymentProvider;

    public SubscriptionService(
        IApplicationDbContext dbContext,
        IPaymentProvider paymentProvider)
    {
        _dbContext = dbContext;
        _paymentProvider = paymentProvider;
    }

    public async Task<SubscriptionStatusResponse> GetStatusAsync(Guid organizationId)
    {
        EnsureValidOrganizationId(organizationId);
        await ExpireOldSubscriptionsAsync();
        await EnsureOrganizationExistsAsync(organizationId);

        var subscription = await _dbContext.Subscriptions
            .AsNoTracking()
            .Where(subscription => subscription.OrganizationId == organizationId)
            .OrderByDescending(subscription => subscription.Status == SubscriptionStatus.Active)
            .ThenByDescending(subscription => subscription.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription is null)
        {
            return new SubscriptionStatusResponse
            {
                OrganizationId = organizationId,
                Status = SubscriptionStatus.Expired.ToString(),
                DaysRemaining = 0,
                IsActive = false
            };
        }

        var now = DateTime.UtcNow;
        var isActive = subscription.Status == SubscriptionStatus.Active
            && subscription.EndDate.HasValue
            && subscription.EndDate.Value > now;

        return new SubscriptionStatusResponse
        {
            OrganizationId = organizationId,
            Status = isActive ? SubscriptionStatus.Active.ToString() : subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            DaysRemaining = CalculateDaysRemaining(subscription.EndDate, now),
            IsActive = isActive
        };
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid organizationId)
    {
        EnsureValidOrganizationId(organizationId);
        await ExpireOldSubscriptionsAsync();

        var now = DateTime.UtcNow;

        return await _dbContext.Subscriptions.AnyAsync(subscription =>
            subscription.OrganizationId == organizationId
            && subscription.Status == SubscriptionStatus.Active
            && subscription.EndDate.HasValue
            && subscription.EndDate.Value > now);
    }

    public async Task<CreateCheckoutResponse> CreateCheckoutAsync(Guid organizationId)
    {
        EnsureValidOrganizationId(organizationId);
        await ExpireOldSubscriptionsAsync();

        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(organization => organization.Id == organizationId);

        if (organization is null)
        {
            throw new KeyNotFoundException("Organization was not found.");
        }

        if (!organization.IsActive)
        {
            throw new ValidationException("Organization is inactive.");
        }

        if (await HasActiveSubscriptionAsync(organizationId))
        {
            throw new ValidationException("Organization already has an active subscription.");
        }

        var checkoutSession = await _paymentProvider.CreateCheckoutSessionAsync(new CreateCheckoutSessionRequest
        {
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            OfficialEmail = organization.OfficialEmail
        });

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Status = SubscriptionStatus.Pending,
            PaymentProvider = PaymentProvider.Stripe,
            PaymentReference = checkoutSession.PaymentReference,
            Amount = checkoutSession.Amount,
            Currency = checkoutSession.Currency,
            CreatedAt = DateTime.UtcNow
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SubscriptionId = subscription.Id,
            Provider = PaymentProvider.Stripe,
            TransactionId = checkoutSession.PaymentReference,
            Status = PaymentStatus.Pending,
            Amount = checkoutSession.Amount,
            Currency = checkoutSession.Currency,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        _dbContext.Payments.Add(payment);

        await SaveChangesAsync();

        return new CreateCheckoutResponse
        {
            CheckoutUrl = checkoutSession.CheckoutUrl,
            PaymentReference = checkoutSession.PaymentReference
        };
    }

    public async Task ActivateSubscriptionAfterPaymentAsync(
        Guid organizationId,
        string paymentReference,
        decimal amount,
        string currency)
    {
        EnsureValidOrganizationId(organizationId);

        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            throw new ValidationException("Payment reference is required.");
        }

        await EnsureOrganizationExistsAsync(organizationId);
        await ExpireOldSubscriptionsAsync();

        var now = DateTime.UtcNow;
        var normalizedCurrency = NormalizeCurrency(currency);

        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(subscription =>
                subscription.OrganizationId == organizationId
                && subscription.PaymentReference == paymentReference);

        if (subscription is null)
        {
            subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                PaymentProvider = PaymentProvider.Stripe,
                PaymentReference = paymentReference,
                CreatedAt = now
            };

            _dbContext.Subscriptions.Add(subscription);
        }

        var otherActiveSubscriptions = await _dbContext.Subscriptions
            .Where(existingSubscription =>
                existingSubscription.OrganizationId == organizationId
                && existingSubscription.Id != subscription.Id
                && existingSubscription.Status == SubscriptionStatus.Active)
            .ToListAsync();

        foreach (var activeSubscription in otherActiveSubscriptions)
        {
            activeSubscription.Status = SubscriptionStatus.Cancelled;
        }

        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDate = now;
        subscription.EndDate = now.AddMonths(3);
        subscription.Amount = amount;
        subscription.Currency = normalizedCurrency;
        subscription.PaymentProvider = PaymentProvider.Stripe;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(payment =>
                payment.Provider == PaymentProvider.Stripe
                && payment.TransactionId == paymentReference);

        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Provider = PaymentProvider.Stripe,
                TransactionId = paymentReference,
                CreatedAt = now
            };

            _dbContext.Payments.Add(payment);
        }

        payment.SubscriptionId = subscription.Id;
        payment.Status = PaymentStatus.Paid;
        payment.Amount = amount;
        payment.Currency = normalizedCurrency;
        payment.PaidAt = now;

        await SaveChangesAsync();
    }

    public async Task ExpireOldSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredSubscriptions = await _dbContext.Subscriptions
            .Where(subscription =>
                subscription.Status == SubscriptionStatus.Active
                && subscription.EndDate.HasValue
                && subscription.EndDate.Value < now)
            .ToListAsync();

        if (expiredSubscriptions.Count == 0)
        {
            return;
        }

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.Status = SubscriptionStatus.Expired;
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureOrganizationExistsAsync(Guid organizationId)
    {
        var organizationExists = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(organization => organization.Id == organizationId);

        if (!organizationExists)
        {
            throw new KeyNotFoundException("Organization was not found.");
        }
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintException(exception))
        {
            throw new ValidationException("Organization already has an active subscription or duplicate payment reference.", exception);
        }
    }

    private static bool IsUniqueConstraintException(DbUpdateException exception)
    {
        return exception.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void EnsureValidOrganizationId(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ValidationException("OrganizationId is required.");
        }
    }

    private static int CalculateDaysRemaining(DateTime? endDate, DateTime now)
    {
        if (!endDate.HasValue || endDate.Value <= now)
        {
            return 0;
        }

        return (int)Math.Ceiling((endDate.Value - now).TotalDays);
    }

    private static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
    }
}
