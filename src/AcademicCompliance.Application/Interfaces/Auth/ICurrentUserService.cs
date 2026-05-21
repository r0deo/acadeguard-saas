namespace AcademicCompliance.Application.Interfaces.Auth;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    string? Email { get; }

    string? Role { get; }

    Guid? OrganizationId { get; }
}
