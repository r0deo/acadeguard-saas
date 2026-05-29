using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.AnalysisRequests;
using AcademicCompliance.Application.DTOs.Uploads;
using AcademicCompliance.Application.Interfaces.AnalysisRequests;
using AcademicCompliance.Application.Interfaces.Uploads;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analysis-requests")]
public sealed class AnalysisRequestsController : ControllerBase
{
    private const long MaximumUploadSizeBytes = 50L * 1024L * 1024L;
    private const string AdminOrOrganizationUser = ApplicationRoles.Admin + "," + ApplicationRoles.OrganizationUser;

    private readonly IAnalysisRequestService _analysisRequestService;
    private readonly IUploadedDocumentService _uploadedDocumentService;

    public AnalysisRequestsController(
        IAnalysisRequestService analysisRequestService,
        IUploadedDocumentService uploadedDocumentService)
    {
        _analysisRequestService = analysisRequestService;
        _uploadedDocumentService = uploadedDocumentService;
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

    [Authorize(Roles = ApplicationRoles.OrganizationUser)]
    [HttpPost("{requestId:guid}/documents")]
    [RequestSizeLimit(MaximumUploadSizeBytes)]
    public async Task<ActionResult<UploadDocumentResponse>> UploadDocument(
        Guid requestId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new ValidationException("File is required.");
        }

        await using var content = file.OpenReadStream();
        var response = await _uploadedDocumentService.UploadAsync(
            requestId,
            new UploadDocumentInput
            {
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                Content = content
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(DocumentsController.GetById),
            "Documents",
            new { documentId = response.DocumentId },
            response);
    }

    [Authorize(Roles = AdminOrOrganizationUser)]
    [HttpGet("{requestId:guid}/documents")]
    public async Task<ActionResult<List<UploadedDocumentResponse>>> GetRequestDocuments(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var response = await _uploadedDocumentService.GetRequestDocumentsAsync(
            requestId,
            cancellationToken);

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
