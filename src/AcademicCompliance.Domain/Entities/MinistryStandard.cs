using AcademicCompliance.Domain.Common;

namespace AcademicCompliance.Domain.Entities;

public class MinistryStandard : BaseEntity
{
    public int Number { get; set; }

    public string? TitleArabic { get; set; }

    public string? TitleEnglish { get; set; }

    public string? DescriptionArabic { get; set; }

    public string? DescriptionEnglish { get; set; }

    public bool IsActive { get; set; } = true;

    public List<MinistryRequirement> Requirements { get; set; } = [];
}
