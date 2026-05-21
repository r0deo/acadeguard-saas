using AcademicCompliance.Domain.Common;

namespace AcademicCompliance.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string OfficialEmail { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? ContactPersonName { get; set; }

    public string? ContactPersonEmail { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
