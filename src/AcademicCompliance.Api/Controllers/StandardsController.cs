using AcademicCompliance.Application.DTOs.Standards;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/standards")]
public sealed class StandardsController : ControllerBase
{
    private readonly IMinistryStandardsService _standardsService;

    public StandardsController(IMinistryStandardsService standardsService)
    {
        _standardsService = standardsService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StandardListItemResponse>>> GetAll()
    {
        var standards = await _standardsService.GetStandardsAsync();

        return Ok(standards);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StandardResponse>> GetById(Guid id)
    {
        var standard = await _standardsService.GetStandardByIdAsync(id);

        return Ok(standard);
    }

    [HttpGet("{id:guid}/tree")]
    public async Task<ActionResult<StandardTreeResponse>> GetTree(Guid id)
    {
        var standard = await _standardsService.GetStandardTreeAsync(id);

        return Ok(standard);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StandardResponse>> Update(Guid id, UpdateStandardRequest request)
    {
        var standard = await _standardsService.UpdateStandardAsync(id, request);

        return Ok(standard);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _standardsService.ActivateStandardAsync(id);

        return NoContent();
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _standardsService.DeactivateStandardAsync(id);

        return NoContent();
    }
}
