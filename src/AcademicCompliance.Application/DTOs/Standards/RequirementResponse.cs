namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class RequirementResponse
{
    public Guid Id { get; init; }

    public Guid StandardId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string? TitleArabic { get; init; }

    public string? TitleEnglish { get; init; }

    public string? TextArabic { get; init; }

    public string? TextEnglish { get; init; }

    public int? PageNumberArabic { get; init; }

    public int? PageNumberEnglish { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public List<ClauseResponse> Clauses { get; init; } = [];
}
