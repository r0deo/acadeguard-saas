namespace AcademicCompliance.Application.DTOs.Uploads;

public sealed class UploadDocumentResponse
{
    public Guid DocumentId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string UploadStatus { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
