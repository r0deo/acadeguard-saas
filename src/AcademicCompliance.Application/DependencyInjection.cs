using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Application.Services.Auth;
using AcademicCompliance.Application.Validators.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicCompliance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthRequestValidator, DataAnnotationsAuthRequestValidator>();

        return services;
    }
}
