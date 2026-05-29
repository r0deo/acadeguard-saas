using AcademicCompliance.Application.DTOs.Uploads;

namespace AcademicCompliance.Application.Interfaces.Uploads;

public interface IUploadedDocumentService
{
    Task<UploadDocumentResponse> UploadAsync(
        Guid analysisRequestId,
        UploadDocumentInput file,
        CancellationToken cancellationToken = default);

    Task<List<UploadedDocumentResponse>> GetRequestDocumentsAsync(
        Guid analysisRequestId,
        CancellationToken cancellationToken = default);

    Task<List<UploadedDocumentResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UploadedDocumentResponse> GetDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
