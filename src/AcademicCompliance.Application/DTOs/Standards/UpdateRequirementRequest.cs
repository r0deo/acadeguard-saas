using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class UpdateRequirementRequest
{
    [Required]
    [StringLength(50)]
    public string Code { get; init; } = string.Empty;

    [StringLength(500)]
    public string? TitleArabic { get; init; }

    [StringLength(500)]
    public string? TitleEnglish { get; init; }

    public string? TextArabic { get; init; }

    public string? TextEnglish { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageNumberArabic { get; init; }

    [Range(1, int.MaxValue)]
    public int? PageNumberEnglish { get; init; }
}
