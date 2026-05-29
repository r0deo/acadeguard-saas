namespace AcademicCompliance.Application.DTOs.Uploads;

public sealed class FileStorageRequest
{
    public Guid OrganizationId { get; set; }

    public Guid AnalysisRequestId { get; set; }

    public string StoredFileName { get; set; } = string.Empty;

    public Stream Content { get; set; } = Stream.Null;
}
