using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcademicCompliance.Application.DTOs.Auth;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AcademicCompliance.Api.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult GenerateToken(ApplicationUser user, string role)
    {
        var jwtSection = _configuration.GetSection("Jwt");
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

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role)
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
        }

        var expires = DateTime.UtcNow.AddMinutes(expirationInMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtTokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expires
        };
    }
}
