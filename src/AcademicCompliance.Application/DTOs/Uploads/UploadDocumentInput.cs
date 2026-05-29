namespace AcademicCompliance.Application.DTOs.Uploads;

public sealed class UploadDocumentInput
{
    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public Stream Content { get; set; } = Stream.Null;
}
