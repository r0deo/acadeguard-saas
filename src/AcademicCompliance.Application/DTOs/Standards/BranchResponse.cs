namespace AcademicCompliance.Application.DTOs.Standards;

public sealed class BranchResponse
{
    public Guid Id { get; init; }

    public Guid ClauseId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string? TextArabic { get; init; }

    public string? TextEnglish { get; init; }

    public int? PageNumberArabic { get; init; }

    public int? PageNumberEnglish { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
