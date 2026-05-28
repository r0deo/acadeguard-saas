using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Standards;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Standards;
using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Standards;

public sealed class MinistryStandardsService : IMinistryStandardsService
{
    private readonly IApplicationDbContext _dbContext;

    public MinistryStandardsService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<StandardListItemResponse>> GetStandardsAsync()
    {
        return await _dbContext.MinistryStandards
            .AsNoTracking()
            .OrderBy(standard => standard.Number)
            .Select(standard => new StandardListItemResponse
            {
                Id = standard.Id,
                Number = standard.Number,
                TitleArabic = standard.TitleArabic,
                TitleEnglish = standard.TitleEnglish,
                IsActive = standard.IsActive,
                CreatedAt = standard.CreatedAt,
                UpdatedAt = standard.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<StandardResponse> GetStandardByIdAsync(Guid id)
    {
        var standard = await GetStandardAsync(id, asTracking: false);

        return MapStandardResponse(standard);
    }

    public async Task<StandardTreeResponse> GetStandardTreeAsync(Guid id)
    {
        var standard = await _dbContext.MinistryStandards
            .AsNoTracking()
            .Include(standard => standard.Requirements)
            .ThenInclude(requirement => requirement.Clauses)
            .ThenInclude(clause => clause.Branches)
            .FirstOrDefaultAsync(standard => standard.Id == id);

        if (standard is null)
        {
            throw new KeyNotFoundException("Ministry standard was not found.");
        }

        return MapStandardTreeResponse(standard);
    }

    public async Task<StandardResponse> UpdateStandardAsync(Guid id, UpdateStandardRequest request)
    {
        ValidateRequest(request);

        var standard = await GetStandardAsync(id, asTracking: true);
        var titleArabic = NormalizeRequired(request.TitleArabic, "TitleArabic is required.");
        var titleEnglish = NormalizeRequired(request.TitleEnglish, "TitleEnglish is required.");

        standard.TitleArabic = titleArabic;
        standard.TitleEnglish = titleEnglish;
        standard.DescriptionArabic = NormalizeOptional(request.DescriptionArabic);
        standard.DescriptionEnglish = NormalizeOptional(request.DescriptionEnglish);

        await SaveChangesAsync();

        return MapStandardResponse(standard);
    }

    public async Task<RequirementResponse> UpdateRequirementAsync(Guid id, UpdateRequirementRequest request)
    {
        ValidateRequest(request);

        var requirement = await GetRequirementAsync(id, asTracking: true);
        var code = NormalizeRequired(request.Code, "Requirement code is required.");
        var titleArabic = NormalizeOptional(request.TitleArabic);
        var titleEnglish = NormalizeOptional(request.TitleEnglish);
        var textArabic = NormalizeOptional(request.TextArabic);
        var textEnglish = NormalizeOptional(request.TextEnglish);

        EnsureHasAnyContent(
            titleArabic,
            titleEnglish,
            textArabic,
            textEnglish,
            "Requirement must have Arabic or English title/text.");
        await EnsureUniqueRequirementCodeAsync(requirement.StandardId, code, id);

        requirement.Code = code;
        requirement.TitleArabic = titleArabic;
        requirement.TitleEnglish = titleEnglish;
        requirement.TextArabic = textArabic;
        requirement.TextEnglish = textEnglish;
        requirement.PageNumberArabic = request.PageNumberArabic;
        requirement.PageNumberEnglish = request.PageNumberEnglish;

        await SaveChangesAsync();

        return MapRequirementResponse(requirement);
    }

    public async Task<ClauseResponse> UpdateClauseAsync(Guid id, UpdateClauseRequest request)
    {
        ValidateRequest(request);

        var clause = await GetClauseAsync(id, asTracking: true);
        var code = NormalizeRequired(request.Code, "Clause code is required.");
        var textArabic = NormalizeOptional(request.TextArabic);
        var textEnglish = NormalizeOptional(request.TextEnglish);

        EnsureHasAnyContent(textArabic, textEnglish, "Clause must have Arabic or English text.");
        await EnsureUniqueClauseCodeAsync(clause.RequirementId, code, id);

        clause.Code = code;
        clause.TextArabic = textArabic;
        clause.TextEnglish = textEnglish;
        clause.PageNumberArabic = request.PageNumberArabic;
        clause.PageNumberEnglish = request.PageNumberEnglish;

        await SaveChangesAsync();

        return MapClauseResponse(clause);
    }

    public async Task<BranchResponse> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
    {
        ValidateRequest(request);

        var branch = await GetBranchAsync(id, asTracking: true);
        var code = NormalizeRequired(request.Code, "Branch code is required.");
        var textArabic = NormalizeOptional(request.TextArabic);
        var textEnglish = NormalizeOptional(request.TextEnglish);

        EnsureHasAnyContent(textArabic, textEnglish, "Branch must have Arabic or English text.");
        await EnsureUniqueBranchCodeAsync(branch.ClauseId, code, id);

        branch.Code = code;
        branch.TextArabic = textArabic;
        branch.TextEnglish = textEnglish;
        branch.PageNumberArabic = request.PageNumberArabic;
        branch.PageNumberEnglish = request.PageNumberEnglish;

        await SaveChangesAsync();

        return MapBranchResponse(branch);
    }

    public async Task ActivateStandardAsync(Guid id)
    {
        var standard = await GetStandardAsync(id, asTracking: true);

        standard.IsActive = true;
        await SaveChangesAsync();
    }

    public async Task DeactivateStandardAsync(Guid id)
    {
        var standard = await GetStandardAsync(id, asTracking: true);

        standard.IsActive = false;
        await SaveChangesAsync();
    }

    public async Task ActivateRequirementAsync(Guid id)
    {
        var requirement = await GetRequirementAsync(id, asTracking: true);

        requirement.IsActive = true;
        await SaveChangesAsync();
    }

    public async Task DeactivateRequirementAsync(Guid id)
    {
        var requirement = await GetRequirementAsync(id, asTracking: true);

        requirement.IsActive = false;
        await SaveChangesAsync();
    }

    public async Task ActivateClauseAsync(Guid id)
    {
        var clause = await GetClauseAsync(id, asTracking: true);

        clause.IsActive = true;
        await SaveChangesAsync();
    }

    public async Task DeactivateClauseAsync(Guid id)
    {
        var clause = await GetClauseAsync(id, asTracking: true);

        clause.IsActive = false;
        await SaveChangesAsync();
    }

    public async Task ActivateBranchAsync(Guid id)
    {
        var branch = await GetBranchAsync(id, asTracking: true);

        branch.IsActive = true;
        await SaveChangesAsync();
    }

    public async Task DeactivateBranchAsync(Guid id)
    {
        var branch = await GetBranchAsync(id, asTracking: true);

        branch.IsActive = false;
        await SaveChangesAsync();
    }

    private async Task<MinistryStandard> GetStandardAsync(Guid id, bool asTracking)
    {
        var query = asTracking
            ? _dbContext.MinistryStandards
            : _dbContext.MinistryStandards.AsNoTracking();

        var standard = await query.FirstOrDefaultAsync(standard => standard.Id == id);
        if (standard is null)
        {
            throw new KeyNotFoundException("Ministry standard was not found.");
        }

        return standard;
    }

    private async Task<MinistryRequirement> GetRequirementAsync(Guid id, bool asTracking)
    {
        var query = asTracking
            ? _dbContext.MinistryRequirements
            : _dbContext.MinistryRequirements.AsNoTracking();

        var requirement = await query.FirstOrDefaultAsync(requirement => requirement.Id == id);
        if (requirement is null)
        {
            throw new KeyNotFoundException("Ministry requirement was not found.");
        }

        return requirement;
    }

    private async Task<MinistryClause> GetClauseAsync(Guid id, bool asTracking)
    {
        var query = asTracking
            ? _dbContext.MinistryClauses
            : _dbContext.MinistryClauses.AsNoTracking();

        var clause = await query.FirstOrDefaultAsync(clause => clause.Id == id);
        if (clause is null)
        {
            throw new KeyNotFoundException("Ministry clause was not found.");
        }

        return clause;
    }

    private async Task<MinistryBranch> GetBranchAsync(Guid id, bool asTracking)
    {
        var query = asTracking
            ? _dbContext.MinistryBranches
            : _dbContext.MinistryBranches.AsNoTracking();

        var branch = await query.FirstOrDefaultAsync(branch => branch.Id == id);
        if (branch is null)
        {
            throw new KeyNotFoundException("Ministry branch was not found.");
        }

        return branch;
    }

    private async Task EnsureUniqueRequirementCodeAsync(Guid standardId, string code, Guid excludedRequirementId)
    {
        var exists = await _dbContext.MinistryRequirements.AnyAsync(requirement =>
            requirement.StandardId == standardId
            && requirement.Code == code
            && requirement.Id != excludedRequirementId);

        if (exists)
        {
            throw new ValidationException("Requirement code already exists within this standard.");
        }
    }

    private async Task EnsureUniqueClauseCodeAsync(Guid requirementId, string code, Guid excludedClauseId)
    {
        var exists = await _dbContext.MinistryClauses.AnyAsync(clause =>
            clause.RequirementId == requirementId
            && clause.Code == code
            && clause.Id != excludedClauseId);

        if (exists)
        {
            throw new ValidationException("Clause code already exists within this requirement.");
        }
    }

    private async Task EnsureUniqueBranchCodeAsync(Guid clauseId, string code, Guid excludedBranchId)
    {
        var exists = await _dbContext.MinistryBranches.AnyAsync(branch =>
            branch.ClauseId == clauseId
            && branch.Code == code
            && branch.Id != excludedBranchId);

        if (exists)
        {
            throw new ValidationException("Branch code already exists within this clause.");
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

    private static void EnsureHasAnyContent(params string?[] values)
    {
        var message = values.Last();
        var contentValues = values.Take(values.Length - 1);

        if (contentValues.Any(value => !string.IsNullOrWhiteSpace(value)))
        {
            return;
        }

        throw new ValidationException(message);
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

    private static StandardResponse MapStandardResponse(MinistryStandard standard)
    {
        return new StandardResponse
        {
            Id = standard.Id,
            Number = standard.Number,
            TitleArabic = standard.TitleArabic,
            TitleEnglish = standard.TitleEnglish,
            DescriptionArabic = standard.DescriptionArabic,
            DescriptionEnglish = standard.DescriptionEnglish,
            IsActive = standard.IsActive,
            CreatedAt = standard.CreatedAt,
            UpdatedAt = standard.UpdatedAt
        };
    }

    private static StandardTreeResponse MapStandardTreeResponse(MinistryStandard standard)
    {
        return new StandardTreeResponse
        {
            Id = standard.Id,
            Number = standard.Number,
            TitleArabic = standard.TitleArabic,
            TitleEnglish = standard.TitleEnglish,
            DescriptionArabic = standard.DescriptionArabic,
            DescriptionEnglish = standard.DescriptionEnglish,
            IsActive = standard.IsActive,
            CreatedAt = standard.CreatedAt,
            UpdatedAt = standard.UpdatedAt,
            Requirements = standard.Requirements
                .OrderBy(requirement => requirement.Code, MinistryCodeComparer.Instance)
                .Select(MapRequirementResponse)
                .ToList()
        };
    }

    private static RequirementResponse MapRequirementResponse(MinistryRequirement requirement)
    {
        return new RequirementResponse
        {
            Id = requirement.Id,
            StandardId = requirement.StandardId,
            Code = requirement.Code,
            TitleArabic = requirement.TitleArabic,
            TitleEnglish = requirement.TitleEnglish,
            TextArabic = requirement.TextArabic,
            TextEnglish = requirement.TextEnglish,
            PageNumberArabic = requirement.PageNumberArabic,
            PageNumberEnglish = requirement.PageNumberEnglish,
            IsActive = requirement.IsActive,
            CreatedAt = requirement.CreatedAt,
            UpdatedAt = requirement.UpdatedAt,
            Clauses = requirement.Clauses
                .OrderBy(clause => clause.Code, MinistryCodeComparer.Instance)
                .Select(MapClauseResponse)
                .ToList()
        };
    }

    private static ClauseResponse MapClauseResponse(MinistryClause clause)
    {
        return new ClauseResponse
        {
            Id = clause.Id,
            RequirementId = clause.RequirementId,
            Code = clause.Code,
            TextArabic = clause.TextArabic,
            TextEnglish = clause.TextEnglish,
            PageNumberArabic = clause.PageNumberArabic,
            PageNumberEnglish = clause.PageNumberEnglish,
            IsActive = clause.IsActive,
            CreatedAt = clause.CreatedAt,
            UpdatedAt = clause.UpdatedAt,
            Branches = clause.Branches
                .OrderBy(branch => branch.Code, MinistryCodeComparer.Instance)
                .Select(MapBranchResponse)
                .ToList()
        };
    }

    private static BranchResponse MapBranchResponse(MinistryBranch branch)
    {
        return new BranchResponse
        {
            Id = branch.Id,
            ClauseId = branch.ClauseId,
            Code = branch.Code,
            TextArabic = branch.TextArabic,
            TextEnglish = branch.TextEnglish,
            PageNumberArabic = branch.PageNumberArabic,
            PageNumberEnglish = branch.PageNumberEnglish,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt
        };
    }

    private async Task SaveChangesAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsDataIntegrityException(exception))
        {
            throw new ValidationException("Ministry standards data violates a uniqueness or integrity rule.", exception);
        }
    }

    private static bool IsDataIntegrityException(DbUpdateException exception)
    {
        return exception.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("check constraint", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) == true;
    }

    private sealed class MinistryCodeComparer : IComparer<string>
    {
        public static readonly MinistryCodeComparer Instance = new();

        public int Compare(string? first, string? second)
        {
            if (ReferenceEquals(first, second))
            {
                return 0;
            }

            if (first is null)
            {
                return -1;
            }

            if (second is null)
            {
                return 1;
            }

            var firstParts = SplitCode(first);
            var secondParts = SplitCode(second);
            var length = Math.Min(firstParts.Count, secondParts.Count);

            for (var index = 0; index < length; index++)
            {
                var firstPart = firstParts[index];
                var secondPart = secondParts[index];

                if (long.TryParse(firstPart, out var firstNumber)
                    && long.TryParse(secondPart, out var secondNumber))
                {
                    var numberComparison = firstNumber.CompareTo(secondNumber);
                    if (numberComparison != 0)
                    {
                        return numberComparison;
                    }

                    continue;
                }

                var textComparison = string.Compare(firstPart, secondPart, StringComparison.OrdinalIgnoreCase);
                if (textComparison != 0)
                {
                    return textComparison;
                }
            }

            return firstParts.Count.CompareTo(secondParts.Count);
        }

        private static List<string> SplitCode(string code)
        {
            var parts = new List<string>();
            var start = 0;
            var previousWasDigit = code.Length > 0 && char.IsDigit(code[0]);

            for (var index = 1; index < code.Length; index++)
            {
                var currentIsDigit = char.IsDigit(code[index]);
                if (currentIsDigit == previousWasDigit)
                {
                    continue;
                }

                parts.Add(code[start..index]);
                start = index;
                previousWasDigit = currentIsDigit;
            }

            parts.Add(code[start..]);
            return parts;
        }
    }
}
