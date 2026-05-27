using AcademicCompliance.Domain.Common;

namespace AcademicCompliance.Domain.Entities;

public class MinistryBranch : BaseEntity
{
    public Guid ClauseId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? TextArabic { get; set; }

    public string? TextEnglish { get; set; }

    public int? PageNumberArabic { get; set; }

    public int? PageNumberEnglish { get; set; }

    public bool IsActive { get; set; } = true;

    public MinistryClause? Clause { get; set; }
}
