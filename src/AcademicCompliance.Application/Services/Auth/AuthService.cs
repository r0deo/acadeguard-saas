using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Auth;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthRequestValidator _validator;
    private readonly ISubscriptionService _subscriptionService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IJwtTokenService jwtTokenService,
        IAuthRequestValidator validator,
        ISubscriptionService subscriptionService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _jwtTokenService = jwtTokenService;
        _validator = validator;
        _subscriptionService = subscriptionService;
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

        if (role == ApplicationRoles.OrganizationUser)
        {
            await EnsureActiveOrganizationUserAsync(user, cancellationToken);
        }

        var token = _jwtTokenService.GenerateToken(user, role);

        return new LoginResponseDto
        {
            AccessToken = token.AccessToken,
            Expiration = token.Expiration,
            Role = role,
            OrganizationId = role == ApplicationRoles.OrganizationUser ? user.OrganizationId : null,
            User = MapUser(user)
        };
    }

    public async Task<OrganizationUserProfileResponse> GetCurrentUserProfileAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.Value.ToString());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive or no longer exists.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("User does not have an assigned role.");

        if (role != ApplicationRoles.OrganizationUser)
        {
            return new OrganizationUserProfileResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                IsActive = user.IsActive
            };
        }

        var organization = await GetActiveUserOrganizationAsync(user, cancellationToken);
        var subscriptionStatus = await _subscriptionService.GetStatusAsync(organization.Id);

        return new OrganizationUserProfileResponse
        {
            UserId = user.Id,
            OrganizationId = organization.Id,
            OrganizationName = organization.Name,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = role,
            IsActive = user.IsActive,
            SubscriptionStatus = subscriptionStatus.Status,
            SubscriptionEndDate = subscriptionStatus.EndDate
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

        var organizationExists = await _dbContext.Organizations.AnyAsync(organization =>
            organization.Id == request.OrganizationId
            && organization.IsActive,
            cancellationToken);

        if (!organizationExists)
        {
            throw new ValidationException("Organization was not found or is inactive.");
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

    private async Task EnsureActiveOrganizationUserAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        await GetActiveUserOrganizationAsync(user, cancellationToken);
    }

    private async Task<Organization> GetActiveUserOrganizationAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        if (!user.OrganizationId.HasValue)
        {
            throw new UnauthorizedAccessException("Organization user is not linked to an organization.");
        }

        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(organization => organization.Id == user.OrganizationId.Value, cancellationToken);

        if (organization is null || !organization.IsActive)
        {
            throw new UnauthorizedAccessException("Organization is inactive or no longer exists.");
        }

        return organization;
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
