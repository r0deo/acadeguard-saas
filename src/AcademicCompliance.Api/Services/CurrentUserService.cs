using System.Security.Claims;
using AcademicCompliance.Application.Interfaces.Auth;

namespace AcademicCompliance.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => CurrentUser?.Identity?.IsAuthenticated == true;

    public Guid? UserId => TryParseGuid(CurrentUser?.FindFirstValue(ClaimTypes.NameIdentifier));

    public string? Email => CurrentUser?.FindFirstValue(ClaimTypes.Email);

    public string? Role => CurrentUser?.FindFirstValue(ClaimTypes.Role);

    public Guid? OrganizationId => TryParseGuid(CurrentUser?.FindFirstValue("organization_id"));

    private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

    private static Guid? TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
