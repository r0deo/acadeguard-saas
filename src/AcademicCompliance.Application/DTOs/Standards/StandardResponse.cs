namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class StandardResponse
{
    public Guid Id { get; init; }

    public int Number { get; init; }

    public string? TitleArabic { get; init; }

    public string? TitleEnglish { get; init; }

    public string? DescriptionArabic { get; init; }

    public string? DescriptionEnglish { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
