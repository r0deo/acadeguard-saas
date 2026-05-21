using Microsoft.AspNetCore.Identity;

namespace AcademicCompliance.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Guid? OrganizationId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
