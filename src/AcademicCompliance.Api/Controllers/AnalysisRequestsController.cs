using AcademicCompliance.Application.DTOs.AnalysisRequests;
using AcademicCompliance.Application.Interfaces.AnalysisRequests;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analysis-requests")]
public sealed class AnalysisRequestsController : ControllerBase
{
    private readonly IAnalysisRequestService _analysisRequestService;

    public AnalysisRequestsController(IAnalysisRequestService analysisRequestService)
    {
        _analysisRequestService = analysisRequestService;
    }

    [Authorize(Roles = ApplicationRoles.OrganizationUser)]
    [HttpPost]
    public async Task<ActionResult<AnalysisRequestResponse>> Create(CreateAnalysisRequestRequest request)
    {
        var response = await _analysisRequestService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize(Roles = ApplicationRoles.OrganizationUser)]
    [HttpGet("my")]
    public async Task<ActionResult<List<AnalysisRequestResponse>>> GetMyRequests()
    {
        var response = await _analysisRequestService.GetMyRequestsAsync();

        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<List<AnalysisRequestResponse>>> GetAll()
    {
        var response = await _analysisRequestService.GetAllAsync();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnalysisRequestResponse>> GetById(Guid id)
    {
        var response = await _analysisRequestService.GetByIdAsync(id);

        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<AnalysisRequestResponse>> UpdateStatus(
        Guid id,
        UpdateAnalysisRequestStatusRequest request)
    {
        var response = await _analysisRequestService.UpdateStatusAsync(id, request);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<AnalysisRequestResponse>> Cancel(Guid id)
    {
        var response = await _analysisRequestService.CancelAsync(id);

        return Ok(response);
    }
}
