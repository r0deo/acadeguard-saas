using AcademicCompliance.Application.Interfaces.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("billing")]
public sealed class BillingRedirectController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public BillingRedirectController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery(Name = "session_id")] string? sessionId)
    {
        var result = await _paymentService.CompleteCheckoutReturnAsync(sessionId ?? string.Empty);

        var title = result.IsActive ? "Payment complete" : "Payment pending";
        return Content(
            BuildHtml(title, result.Message, result.Status),
            "text/html");
    }

    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        return Content(
            BuildHtml(
                "Payment cancelled",
                "Payment was cancelled. Your organization registration remains available, but the subscription is not active.",
                "Pending"),
            "text/html");
    }

    private static string BuildHtml(string title, string message, string status)
    {
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>{{title}}</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 48px; color: #172033; }
                    main { max-width: 720px; }
                    h1 { font-size: 28px; margin-bottom: 12px; }
                    p { font-size: 16px; line-height: 1.5; }
                    .status { font-weight: 700; }
                </style>
            </head>
            <body>
                <main>
                    <h1>{{title}}</h1>
                    <p>{{message}}</p>
                    <p>Subscription status: <span class="status">{{status}}</span></p>
                </main>
            </body>
            </html>
            """;
    }
}
