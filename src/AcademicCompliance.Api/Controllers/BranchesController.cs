using AcademicCompliance.Application.DTOs.Standards;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("api/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly IMinistryStandardsService _standardsService;

    public BranchesController(IMinistryStandardsService standardsService)
    {
        _standardsService = standardsService;
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchResponse>> Update(Guid id, UpdateBranchRequest request)
    {
        var branch = await _standardsService.UpdateBranchAsync(id, request);

        return Ok(branch);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _standardsService.ActivateBranchAsync(id);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _standardsService.DeactivateBranchAsync(id);

        return NoContent();
    }
}
