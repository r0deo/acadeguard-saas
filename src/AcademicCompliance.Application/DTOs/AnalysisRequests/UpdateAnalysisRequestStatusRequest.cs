using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.AnalysisRequests;

public sealed class UpdateAnalysisRequestStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; init; } = string.Empty;
}
