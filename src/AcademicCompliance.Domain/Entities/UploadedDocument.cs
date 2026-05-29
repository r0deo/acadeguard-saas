using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Enums;

namespace AcademicCompliance.Domain.Entities;

public class UploadedDocument : BaseEntity
{
    public Guid AnalysisRequestId { get; set; }

    public AnalysisRequest? AnalysisRequest { get; set; }

    public Guid OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DocumentUploadStatus UploadStatus { get; set; } = DocumentUploadStatus.Uploaded;

    public Guid UploadedByUserId { get; set; }

    public ApplicationUser? UploadedByUser { get; set; }
}
