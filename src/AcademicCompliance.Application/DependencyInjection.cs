using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Application.Interfaces.Organizations;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Application.Services.Auth;
using AcademicCompliance.Application.Services.Billing;
using AcademicCompliance.Application.Services.Organizations;
using AcademicCompliance.Application.Services.Standards;
using AcademicCompliance.Application.Validators.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicCompliance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationRegistrationService, OrganizationRegistrationService>();
        services.AddScoped<IAuthRequestValidator, DataAnnotationsAuthRequestValidator>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IMinistryStandardsService, MinistryStandardsService>();

        return services;
    }
}
