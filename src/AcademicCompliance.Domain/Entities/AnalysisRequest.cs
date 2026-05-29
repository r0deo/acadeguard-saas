using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Enums;

namespace AcademicCompliance.Domain.Entities;

public class AnalysisRequest : BaseEntity
{
    public Guid OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public Guid CreatedByUserId { get; set; }

    public ApplicationUser? CreatedByUser { get; set; }

    public string Semester { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Notes { get; set; }

    public AnalysisRequestStatus Status { get; set; } = AnalysisRequestStatus.Submitted;
}
