using AcademicCompliance.Domain.Common;

namespace AcademicCompliance.Domain.Entities;

public class MinistryRequirement : BaseEntity
{
    public Guid StandardId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? TitleArabic { get; set; }

    public string? TitleEnglish { get; set; }

    public string? TextArabic { get; set; }

    public string? TextEnglish { get; set; }

    public int? PageNumberArabic { get; set; }

    public int? PageNumberEnglish { get; set; }

    public bool IsActive { get; set; } = true;

    public MinistryStandard? Standard { get; set; }

    public List<MinistryClause> Clauses { get; set; } = [];
}
