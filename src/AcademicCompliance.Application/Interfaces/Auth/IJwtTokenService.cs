using AcademicCompliance.Application.DTOs.Auth;
using AcademicCompliance.Domain.Entities;

namespace AcademicCompliance.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(ApplicationUser user, string role);
}
