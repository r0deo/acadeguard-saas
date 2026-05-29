namespace AcademicCompliance.Application.DTOs.Uploads;

public sealed class UploadedDocumentResponse
{
    public Guid Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string UploadStatus { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}
