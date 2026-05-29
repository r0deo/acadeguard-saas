using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.AnalysisRequests;

public sealed class CreateAnalysisRequestRequest
{
    [Required]
    [StringLength(100)]
    public string Semester { get; init; } = string.Empty;

    [StringLength(250)]
    public string? Title { get; init; }

    [StringLength(1000)]
    public string? Notes { get; init; }
}
