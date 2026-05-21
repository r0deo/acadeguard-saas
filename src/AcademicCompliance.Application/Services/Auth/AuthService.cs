using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Auth;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AcademicCompliance.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthRequestValidator _validator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IJwtTokenService jwtTokenService,
        IAuthRequestValidator validator)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _jwtTokenService = jwtTokenService;
        _validator = validator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        _validator.Validate(request);

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordIsValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("User does not have an assigned role.");

        var token = _jwtTokenService.GenerateToken(user, role);

        return new LoginResponseDto
        {
            AccessToken = token.AccessToken,
            Expiration = token.Expiration,
            Role = role,
            User = MapUser(user)
        };
    }

    public async Task<AuthenticatedUserDto> RegisterOrganizationUserAsync(
        RegisterOrganizationUserDto request,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(request);

        if (_currentUserService.Role != ApplicationRoles.Admin)
        {
            throw new UnauthorizedAccessException("Only administrators can create organization users.");
        }

        var email = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ValidationException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            IsActive = true,
            OrganizationId = request.OrganizationId,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        EnsureIdentitySucceeded(createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.OrganizationUser);
        EnsureIdentitySucceeded(roleResult);

        return MapUser(user);
    }

    private static AuthenticatedUserDto MapUser(ApplicationUser user)
    {
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            OrganizationId = user.OrganizationId,
            IsActive = user.IsActive
        };
    }

    private static void EnsureIdentitySucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new ValidationException(errors);
    }
}
