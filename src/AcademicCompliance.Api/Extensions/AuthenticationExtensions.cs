using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AcademicCompliance.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured.");

        if (!int.TryParse(jwtSection["ExpirationInMinutes"], out var expirationInMinutes)
            || expirationInMinutes <= 0)
        {
            throw new InvalidOperationException("JWT ExpirationInMinutes must be a positive integer.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
