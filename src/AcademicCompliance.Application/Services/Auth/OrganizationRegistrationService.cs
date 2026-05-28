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

public sealed class OrganizationRegistrationService : IOrganizationRegistrationService
{
    private const string RegistrationSuccessMessage =
        "Organization registered successfully. Complete payment to activate subscription.";

    private readonly IApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IAuthRequestValidator _validator;

    public OrganizationRegistrationService(
        IApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IPaymentService paymentService,
        IAuthRequestValidator validator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _paymentService = paymentService;
        _validator = validator;
    }

    public async Task<RegisterOrganizationResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request)
    {
        _validator.Validate(request);

        var organizationName = NormalizeRequired(request.OrganizationName, "Organization name is required.");
        var officialEmail = NormalizeRequired(request.OfficialEmail, "Official email is required.");
        var adminEmail = NormalizeRequired(request.AdminEmail, "Admin email is required.");

        await EnsureUniqueOrganizationNameAsync(organizationName);
        await EnsureUniqueOfficialEmailAsync(officialEmail);
        await EnsureUniqueUserEmailAsync(adminEmail);

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = organizationName,
            OfficialEmail = officialEmail,
            LogoUrl = NormalizeOptional(request.LogoUrl),
            ContactPersonName = NormalizeRequired(request.ContactPersonName, "Contact person name is required."),
            ContactPersonEmail = NormalizeOptional(request.ContactPersonEmail),
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            Address = NormalizeOptional(request.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Organizations.Add(organization);
        await SaveChangesAsync("Organization name or official email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            NormalizedUserName = adminEmail.ToUpperInvariant(),
            Email = adminEmail,
            NormalizedEmail = adminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            FullName = NormalizeRequired(request.AdminFullName, "Admin full name is required."),
            IsActive = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            EnsureIdentitySucceeded(createUserResult);

            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.OrganizationUser);
            EnsureIdentitySucceeded(roleResult);
        }
        catch
        {
            await RollbackOrganizationAndUserAsync(organization, user);
            throw;
        }

        try
        {
            var checkout = await _paymentService.CreateCheckoutAsync(organization.Id);

            return new RegisterOrganizationResponse
            {
                OrganizationId = organization.Id,
                UserId = user.Id,
                CheckoutUrl = checkout.CheckoutUrl,
                PaymentReference = checkout.PaymentReference,
                Message = RegistrationSuccessMessage
            };
        }
        catch (Exception exception) when (exception is not ValidationException)
        {
            throw new ValidationException(
                "Organization registered successfully, but checkout session creation failed. Please retry payment from billing.",
                exception);
        }
        catch (ValidationException exception)
        {
            throw new ValidationException(
                $"Organization registered successfully, but checkout session creation failed: {exception.Message}",
                exception);
        }
    }

    private async Task EnsureUniqueOrganizationNameAsync(string organizationName)
    {
        var normalizedName = organizationName.ToLowerInvariant();
        var exists = await _dbContext.Organizations.AnyAsync(organization =>
            organization.Name.ToLower() == normalizedName);

        if (exists)
        {
            throw new ValidationException("Organization name already exists.");
        }
    }

    private async Task EnsureUniqueOfficialEmailAsync(string officialEmail)
    {
        var normalizedEmail = officialEmail.ToLowerInvariant();
        var exists = await _dbContext.Organizations.AnyAsync(organization =>
            organization.OfficialEmail.ToLower() == normalizedEmail);

        if (exists)
        {
            throw new ValidationException("Official email already exists.");
        }
    }

    private async Task EnsureUniqueUserEmailAsync(string email)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ValidationException("Admin email is already registered.");
        }
    }

    private async Task RollbackOrganizationAndUserAsync(Organization organization, ApplicationUser user)
    {
        var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (existingUser is not null)
        {
            var deleteUserResult = await _userManager.DeleteAsync(existingUser);
            EnsureIdentitySucceeded(deleteUserResult);
        }

        _dbContext.Organizations.Remove(organization);
        await SaveChangesAsync("Could not rollback organization registration.");
    }

    private async Task SaveChangesAsync(string uniqueConstraintMessage)
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintException(exception))
        {
            throw new ValidationException(uniqueConstraintMessage, exception);
        }
    }

    private static bool IsUniqueConstraintException(DbUpdateException exception)
    {
        return exception.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string NormalizeRequired(string value, string errorMessage)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException(errorMessage);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
