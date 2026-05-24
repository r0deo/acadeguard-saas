using AcademicCompliance.Application.DTOs.Billing;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPaymentService _paymentService;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IPaymentService paymentService)
    {
        _subscriptionService = subscriptionService;
        _paymentService = paymentService;
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<SubscriptionStatusResponse>> GetOrganizationStatus(Guid organizationId)
    {
        var status = await _subscriptionService.GetStatusAsync(organizationId);

        return Ok(status);
    }

    [HttpPost("create-checkout")]
    public async Task<ActionResult<CreateCheckoutResponse>> CreateCheckout(CreateCheckoutRequest request)
    {
        var response = await _subscriptionService.CreateCheckoutAsync(request.OrganizationId);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("stripe/webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _paymentService.HandleWebhookAsync(payload, signature);

        return Ok();
    }
}
