using AcademicCompliance.Application.DTOs.Billing;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("organization/{organizationId:guid}")]
    public async Task<ActionResult<List<PaymentResponse>>> GetByOrganization(Guid organizationId)
    {
        var payments = await _paymentService.GetByOrganizationAsync(organizationId);

        return Ok(payments);
    }
}
