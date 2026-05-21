using AcademicCompliance.Application.DTOs.Auth;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost("register-organization-user")]
    public async Task<ActionResult<AuthenticatedUserDto>> RegisterOrganizationUser(
        RegisterOrganizationUserDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterOrganizationUserAsync(request, cancellationToken);

        return Ok(response);
    }
}
