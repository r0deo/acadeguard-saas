using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class UpdateStandardRequest
{
    [Range(1, int.MaxValue)]
    public int Number { get; init; }

    [StringLength(250)]
    public string? TitleArabic { get; init; }

    [StringLength(250)]
    public string? TitleEnglish { get; init; }

    [StringLength(2000)]
    public string? DescriptionArabic { get; init; }

    [StringLength(2000)]
    public string? DescriptionEnglish { get; init; }
}
