using AcademicCompliance.Api.Services;
using AcademicCompliance.Application.Interfaces.Auth;

namespace AcademicCompliance.Api.Extensions;

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
