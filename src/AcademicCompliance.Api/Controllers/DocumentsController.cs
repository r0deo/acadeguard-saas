using AcademicCompliance.Application.DTOs.Uploads;
using AcademicCompliance.Application.Interfaces.Uploads;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private const string AdminOrOrganizationUser = ApplicationRoles.Admin + "," + ApplicationRoles.OrganizationUser;

    private readonly IUploadedDocumentService _uploadedDocumentService;

    public DocumentsController(IUploadedDocumentService uploadedDocumentService)
    {
        _uploadedDocumentService = uploadedDocumentService;
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<List<UploadedDocumentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _uploadedDocumentService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = AdminOrOrganizationUser)]
    [HttpGet("{documentId:guid}")]
    public async Task<ActionResult<UploadedDocumentResponse>> GetById(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var response = await _uploadedDocumentService.GetDocumentAsync(
            documentId,
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = AdminOrOrganizationUser)]
    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await _uploadedDocumentService.DeleteDocumentAsync(documentId, cancellationToken);

        return NoContent();
    }
}
