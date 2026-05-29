using AcademicCompliance.Application.Interfaces.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("billing")]
public sealed class BillingRedirectController : ControllerBase
{
    private const string DefaultClientBaseUrl = "http://localhost:5173";
    private const string DefaultSuccessPath = "/upload";
    private const string DefaultPendingPath = "/billing";
    private const string DefaultCancelPath = "/billing";

    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public BillingRedirectController(
        IPaymentService paymentService,
        IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery(Name = "session_id")] string? sessionId)
    {
        var result = await _paymentService.CompleteCheckoutReturnAsync(sessionId ?? string.Empty);
        var path = result.IsActive
            ? GetConfiguredPath("BillingRedirects:SuccessPath", DefaultSuccessPath)
            : GetConfiguredPath("BillingRedirects:PendingPath", DefaultPendingPath);

        return Redirect(BuildClientRedirectUrl(path, new Dictionary<string, string?>
        {
            ["payment"] = result.IsActive ? "success" : "pending",
            ["subscriptionStatus"] = result.Status,
            ["session_id"] = result.PaymentReference
        }));
    }

    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        var path = GetConfiguredPath("BillingRedirects:CancelPath", DefaultCancelPath);

        return Redirect(BuildClientRedirectUrl(path, new Dictionary<string, string?>
        {
            ["payment"] = "cancelled",
            ["subscriptionStatus"] = "Pending"
        }));
    }

    private string BuildClientRedirectUrl(string path, IReadOnlyDictionary<string, string?> queryParameters)
    {
        var clientBaseUrl = GetConfiguredClientBaseUrl();
        var builder = new UriBuilder(new Uri(new Uri(clientBaseUrl), path));
        var query = string.Join("&", queryParameters
            .Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value))
            .Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value!)}"));

        builder.Query = query;

        return builder.Uri.ToString();
    }

    private string GetConfiguredClientBaseUrl()
    {
        var configuredUrl = _configuration["ClientApp:BaseUrl"];
        if (Uri.TryCreate(configuredUrl, UriKind.Absolute, out var clientUri)
            && (clientUri.Scheme == Uri.UriSchemeHttp || clientUri.Scheme == Uri.UriSchemeHttps))
        {
            return clientUri.ToString().TrimEnd('/');
        }

        return DefaultClientBaseUrl;
    }

    private string GetConfiguredPath(string key, string fallback)
    {
        var configuredPath = _configuration[key];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return fallback;
        }

        return configuredPath.StartsWith('/') ? configuredPath : $"/{configuredPath}";
    }
}
