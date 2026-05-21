using AcademicCompliance.Application.DTOs.Organizations;
using AcademicCompliance.Application.Interfaces.Organizations;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/organizations")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrganizationListItemResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive)
    {
        var organizations = await _organizationService.GetAllAsync(search, isActive);

        return Ok(organizations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> GetById(Guid id)
    {
        var organization = await _organizationService.GetByIdAsync(id);

        return Ok(organization);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<OrganizationResponse>> Create(CreateOrganizationRequest request)
    {
        var organization = await _organizationService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> Update(Guid id, UpdateOrganizationRequest request)
    {
        var organization = await _organizationService.UpdateAsync(id, request);

        return Ok(organization);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _organizationService.ActivateAsync(id);

        return NoContent();
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _organizationService.DeactivateAsync(id);

        return NoContent();
    }
}
