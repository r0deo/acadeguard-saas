using AcademicCompliance.Application.DTOs.Standards;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize(Roles = ApplicationRoles.Admin)]
[Route("api/clauses")]
public sealed class ClausesController : ControllerBase
{
    private readonly IMinistryStandardsService _standardsService;

    public ClausesController(IMinistryStandardsService standardsService)
    {
        _standardsService = standardsService;
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClauseResponse>> Update(Guid id, UpdateClauseRequest request)
    {
        var clause = await _standardsService.UpdateClauseAsync(id, request);

        return Ok(clause);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _standardsService.ActivateClauseAsync(id);

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _standardsService.DeactivateClauseAsync(id);

        return NoContent();
    }
}
