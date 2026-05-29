namespace AcademicCompliance.Application.DTOs.AnalysisRequests;

public sealed class AnalysisRequestResponse
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public string OrganizationName { get; init; } = string.Empty;

    public Guid CreatedByUserId { get; init; }

    public string CreatedByUserEmail { get; init; } = string.Empty;

    public string Semester { get; init; } = string.Empty;

    public string? Title { get; init; }

    public string? Notes { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
