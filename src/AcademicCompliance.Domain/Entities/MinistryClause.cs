using AcademicCompliance.Domain.Common;

namespace AcademicCompliance.Domain.Entities;

public class MinistryClause : BaseEntity
{
    public Guid RequirementId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? TextArabic { get; set; }

    public string? TextEnglish { get; set; }

    public int? PageNumberArabic { get; set; }

    public int? PageNumberEnglish { get; set; }

    public bool IsActive { get; set; } = true;

    public MinistryRequirement? Requirement { get; set; }

    public List<MinistryBranch> Branches { get; set; } = [];
}
