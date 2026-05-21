namespace AcademicCompliance.Domain.Common;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string OrganizationUser = "OrganizationUser";

    public static readonly string[] All =
    [
        Admin,
        OrganizationUser
    ];
}
