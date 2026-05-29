namespace AcademicCompliance.Domain.Enums;

public enum DocumentUploadStatus
{
    Uploaded = 1,
    ReadyForParsing = 2,
    Parsing = 3,
    Parsed = 4,
    Failed = 5
}
