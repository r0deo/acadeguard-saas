using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Organizations;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Organizations;
using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Organizations;

public sealed class OrganizationService : IOrganizationService
{
    private readonly IApplicationDbContext _dbContext;

    public OrganizationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<OrganizationListItemResponse>> GetAllAsync(string? search, bool? isActive)
    {
        var query = _dbContext.Organizations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(organization =>
                organization.Name.ToLower().Contains(searchTerm)
                || organization.OfficialEmail.ToLower().Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(organization => organization.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(organization => organization.Name)
            .Select(organization => new OrganizationListItemResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                OfficialEmail = organization.OfficialEmail,
                ContactPersonName = organization.ContactPersonName,
                IsActive = organization.IsActive,
                CreatedAt = organization.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrganizationResponse> GetByIdAsync(Guid id)
    {
        var organization = await GetOrganizationAsync(id, asTracking: false);

        return MapResponse(organization);
    }

    public async Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request)
    {
        ValidateRequest(request);

        var name = NormalizeRequired(request.Name);
        var officialEmail = NormalizeRequired(request.OfficialEmail);

        await EnsureUniqueNameAsync(name);
        await EnsureUniqueOfficialEmailAsync(officialEmail);

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            OfficialEmail = officialEmail,
            LogoUrl = NormalizeOptional(request.LogoUrl),
            ContactPersonName = NormalizeOptional(request.ContactPersonName),
            ContactPersonEmail = NormalizeOptional(request.ContactPersonEmail),
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            Address = NormalizeOptional(request.Address),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Organizations.Add(organization);
        await SaveChangesAsync();

        return MapResponse(organization);
    }

    public async Task<OrganizationResponse> UpdateAsync(Guid id, UpdateOrganizationRequest request)
    {
        ValidateRequest(request);

        var organization = await GetOrganizationAsync(id, asTracking: true);
        var name = NormalizeRequired(request.Name);
        var officialEmail = NormalizeRequired(request.OfficialEmail);

        await EnsureUniqueNameAsync(name, id);
        await EnsureUniqueOfficialEmailAsync(officialEmail, id);

        organization.Name = name;
        organization.OfficialEmail = officialEmail;
        organization.LogoUrl = NormalizeOptional(request.LogoUrl);
        organization.ContactPersonName = NormalizeOptional(request.ContactPersonName);
        organization.ContactPersonEmail = NormalizeOptional(request.ContactPersonEmail);
        organization.PhoneNumber = NormalizeOptional(request.PhoneNumber);
        organization.Address = NormalizeOptional(request.Address);

        await SaveChangesAsync();

        return MapResponse(organization);
    }

    public async Task ActivateAsync(Guid id)
    {
        var organization = await GetOrganizationAsync(id, asTracking: true);

        organization.IsActive = true;
        await SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var organization = await GetOrganizationAsync(id, asTracking: true);

        organization.IsActive = false;
        await SaveChangesAsync();
    }

    private async Task<Organization> GetOrganizationAsync(Guid id, bool asTracking)
    {
        var query = asTracking
            ? _dbContext.Organizations
            : _dbContext.Organizations.AsNoTracking();

        var organization = await query.FirstOrDefaultAsync(organization => organization.Id == id);
        if (organization is null)
        {
            throw new KeyNotFoundException("Organization was not found.");
        }

        return organization;
    }

    private async Task EnsureUniqueNameAsync(string name, Guid? excludedOrganizationId = null)
    {
        var normalizedName = name.ToLower();
        var exists = await _dbContext.Organizations.AnyAsync(organization =>
            organization.Name.ToLower() == normalizedName
            && (!excludedOrganizationId.HasValue || organization.Id != excludedOrganizationId.Value));

        if (exists)
        {
            throw new ValidationException("Organization name already exists.");
        }
    }

    private async Task EnsureUniqueOfficialEmailAsync(string officialEmail, Guid? excludedOrganizationId = null)
    {
        var normalizedEmail = officialEmail.ToLower();
        var exists = await _dbContext.Organizations.AnyAsync(organization =>
            organization.OfficialEmail.ToLower() == normalizedEmail
            && (!excludedOrganizationId.HasValue || organization.Id != excludedOrganizationId.Value));

        if (exists)
        {
            throw new ValidationException("Official email already exists.");
        }
    }

    private static void ValidateRequest<TRequest>(TRequest request)
    {
        if (request is null)
        {
            throw new ValidationException("Request body is required.");
        }

        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();

        if (Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            return;
        }

        var message = string.Join(" ", validationResults.Select(result => result.ErrorMessage));
        throw new ValidationException(message);
    }

    private static string NormalizeRequired(string value)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("Required fields cannot be empty.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static OrganizationResponse MapResponse(Organization organization)
    {
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            OfficialEmail = organization.OfficialEmail,
            LogoUrl = organization.LogoUrl,
            ContactPersonName = organization.ContactPersonName,
            ContactPersonEmail = organization.ContactPersonEmail,
            PhoneNumber = organization.PhoneNumber,
            Address = organization.Address,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintException(exception))
        {
            throw new ValidationException("Organization name or official email already exists.", exception);
        }
    }

    private static bool IsUniqueConstraintException(DbUpdateException exception)
    {
        return exception.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
    }
}
