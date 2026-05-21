using AcademicCompliance.Application.DTOs.Auth;

namespace AcademicCompliance.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto> RegisterOrganizationUserAsync(
        RegisterOrganizationUserDto request,
        CancellationToken cancellationToken = default);
}
