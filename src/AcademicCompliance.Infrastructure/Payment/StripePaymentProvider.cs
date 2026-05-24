using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AcademicCompliance.Application.DTOs.Billing;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AcademicCompliance.Infrastructure.Payment;

public sealed class StripePaymentProvider : IPaymentProvider
{
    private static readonly HttpClient HttpClient = new();

    private readonly IConfiguration _configuration;

    public StripePaymentProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
    {
        var settings = GetSettings();

        if (IsPlaceholderSecret(settings.SecretKey))
        {
            var paymentReference = $"cs_test_{Guid.NewGuid():N}";

            return new CreateCheckoutSessionResponse
            {
                CheckoutUrl = $"https://checkout.stripe.com/c/pay/{paymentReference}",
                PaymentReference = paymentReference,
                Amount = settings.SubscriptionAmount,
                Currency = settings.Currency
            };
        }

        var amountInMinorUnits = ConvertAmountToMinorUnits(settings.SubscriptionAmount);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);
        httpRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = settings.SuccessUrl,
            ["cancel_url"] = settings.CancelUrl,
            ["client_reference_id"] = request.OrganizationId.ToString(),
            ["customer_email"] = request.OfficialEmail,
            ["metadata[organizationId]"] = request.OrganizationId.ToString(),
            ["metadata[organizationName]"] = request.OrganizationName,
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = settings.Currency.ToLowerInvariant(),
            ["line_items[0][price_data][unit_amount]"] = amountInMinorUnits.ToString(CultureInfo.InvariantCulture),
            ["line_items[0][price_data][product_data][name]"] = "Academic Compliance Verification Subscription",
            ["line_items[0][price_data][product_data][description]"] = "3-month unlimited compliance analysis subscription"
        });

        using var response = await HttpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ValidationException($"Stripe checkout session creation failed: {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        return new CreateCheckoutSessionResponse
        {
            CheckoutUrl = GetRequiredString(root, "url"),
            PaymentReference = GetRequiredString(root, "id"),
            Amount = settings.SubscriptionAmount,
            Currency = settings.Currency
        };
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentReference)
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            throw new ValidationException("Payment reference is required.");
        }

        var settings = GetSettings();

        if (IsPlaceholderSecret(settings.SecretKey))
        {
            return new PaymentVerificationResult
            {
                IsSuccessful = paymentReference.StartsWith("cs_test_", StringComparison.OrdinalIgnoreCase),
                Provider = PaymentProvider.Stripe,
                PaymentReference = paymentReference,
                TransactionId = paymentReference,
                Amount = settings.SubscriptionAmount,
                Currency = settings.Currency
            };
        }

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.stripe.com/v1/checkout/sessions/{Uri.EscapeDataString(paymentReference)}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);

        using var response = await HttpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ValidationException($"Stripe payment verification failed: {responseBody}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var paymentStatus = GetOptionalString(root, "payment_status");
        var amount = GetAmountFromMinorUnits(root, "amount_total", settings.SubscriptionAmount);
        var currency = GetOptionalString(root, "currency")?.ToUpperInvariant() ?? settings.Currency;

        return new PaymentVerificationResult
        {
            IsSuccessful = string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase),
            Provider = PaymentProvider.Stripe,
            PaymentReference = paymentReference,
            TransactionId = GetOptionalString(root, "payment_intent") ?? paymentReference,
            Amount = amount,
            Currency = currency
        };
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        await ParseWebhookAsync(payload, signature);
    }

    public Task<PaymentWebhookResult?> ParseWebhookAsync(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ValidationException("Webhook payload is required.");
        }

        var settings = GetSettings();
        ValidateWebhookSignature(payload, signature, settings.WebhookSecret);

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var eventType = GetOptionalString(root, "type");

        if (!string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<PaymentWebhookResult?>(null);
        }

        var session = root
            .GetProperty("data")
            .GetProperty("object");

        var paymentStatus = GetOptionalString(session, "payment_status");
        var paymentReference = GetRequiredString(session, "id");
        var organizationId = GetOrganizationId(session);
        var amount = GetAmountFromMinorUnits(session, "amount_total", settings.SubscriptionAmount);
        var currency = GetOptionalString(session, "currency")?.ToUpperInvariant() ?? settings.Currency;

        var result = new PaymentWebhookResult
        {
            IsPaymentSuccessful = string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(paymentStatus),
            OrganizationId = organizationId,
            Provider = PaymentProvider.Stripe,
            PaymentReference = paymentReference,
            TransactionId = GetOptionalString(session, "payment_intent") ?? paymentReference,
            Amount = amount,
            Currency = currency
        };

        return Task.FromResult<PaymentWebhookResult?>(result);
    }

    private StripeSettings GetSettings()
    {
        var section = _configuration.GetSection("Stripe");
        var secretKey = section["SecretKey"] ?? string.Empty;
        var webhookSecret = section["WebhookSecret"] ?? string.Empty;
        var successUrl = section["SuccessUrl"] ?? string.Empty;
        var cancelUrl = section["CancelUrl"] ?? string.Empty;
        var currency = section["Currency"] ?? "USD";

        if (!decimal.TryParse(
                section["SubscriptionAmount"],
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var subscriptionAmount)
            || subscriptionAmount <= 0)
        {
            throw new InvalidOperationException("Stripe SubscriptionAmount must be a positive decimal.");
        }

        return new StripeSettings(
            secretKey,
            webhookSecret,
            successUrl,
            cancelUrl,
            subscriptionAmount,
            currency.Trim().ToUpperInvariant());
    }

    private static Guid GetOrganizationId(JsonElement session)
    {
        var organizationId = GetOptionalString(session, "client_reference_id");

        if (string.IsNullOrWhiteSpace(organizationId)
            && session.TryGetProperty("metadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object)
        {
            organizationId = GetOptionalString(metadata, "organizationId");
        }

        if (!Guid.TryParse(organizationId, out var parsedOrganizationId))
        {
            throw new ValidationException("Stripe webhook is missing a valid organization id.");
        }

        return parsedOrganizationId;
    }

    private static void ValidateWebhookSignature(string payload, string signature, string webhookSecret)
    {
        if (IsPlaceholderSecret(webhookSecret))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new UnauthorizedAccessException("Stripe webhook signature is required.");
        }

        var timestamp = GetSignaturePart(signature, "t");
        var expectedSignature = GetSignaturePart(signature, "v1");

        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(expectedSignature))
        {
            throw new UnauthorizedAccessException("Stripe webhook signature is invalid.");
        }

        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        var computedBytes = Encoding.UTF8.GetBytes(computedSignature);

        if (expectedBytes.Length != computedBytes.Length
            || !CryptographicOperations.FixedTimeEquals(expectedBytes, computedBytes))
        {
            throw new UnauthorizedAccessException("Stripe webhook signature is invalid.");
        }
    }

    private static string? GetSignaturePart(string signature, string key)
    {
        return signature
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .Where(parts => string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            .Select(parts => parts[1])
            .FirstOrDefault();
    }

    private static bool IsPlaceholderSecret(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            || value.Contains("replace_me", StringComparison.OrdinalIgnoreCase);
    }

    private static long ConvertAmountToMinorUnits(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero));
    }

    private static decimal GetAmountFromMinorUnits(JsonElement element, string propertyName, decimal fallback)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
        {
            return fallback;
        }

        return property.GetDecimal() / 100m;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var value = GetOptionalString(element, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"Stripe response is missing '{propertyName}'.");
        }

        return value;
    }

    private static string? GetOptionalString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private sealed record StripeSettings(
        string SecretKey,
        string WebhookSecret,
        string SuccessUrl,
        string CancelUrl,
        decimal SubscriptionAmount,
        string Currency);
}
