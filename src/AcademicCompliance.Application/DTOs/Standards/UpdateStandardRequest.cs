using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class UpdateStandardRequest
{
    [Required]
    [StringLength(250)]
    public required string TitleArabic { get; init; }

    [Required]
    [StringLength(250)]
    public required string TitleEnglish { get; init; }

    [StringLength(2000)]
    public required string? DescriptionArabic { get; init; }

    [StringLength(2000)]
    public required string? DescriptionEnglish { get; init; }
}
