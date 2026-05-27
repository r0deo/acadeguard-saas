using AcademicCompliance.Application.DTOs.Standards;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("api/requirements")]
public sealed class RequirementsController : ControllerBase
{
    private readonly IMinistryStandardsService _standardsService;

    public RequirementsController(IMinistryStandardsService standardsService)
    {
        _standardsService = standardsService;
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequirementResponse>> Update(Guid id, UpdateRequirementRequest request)
    {
        var requirement = await _standardsService.UpdateRequirementAsync(id, request);

        return Ok(requirement);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _standardsService.ActivateRequirementAsync(id);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _standardsService.DeactivateRequirementAsync(id);

        return NoContent();
    }
}
