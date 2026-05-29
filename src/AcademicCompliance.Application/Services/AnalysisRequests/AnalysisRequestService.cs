using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.AnalysisRequests;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.AnalysisRequests;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using AcademicCompliance.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.AnalysisRequests;

public sealed class AnalysisRequestService : IAnalysisRequestService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AnalysisRequestService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        ISubscriptionService subscriptionService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _subscriptionService = subscriptionService;
        _userManager = userManager;
    }

    public async Task<AnalysisRequestResponse> CreateAsync(CreateAnalysisRequestRequest request)
    {
        ValidateRequest(request);

        var userId = GetRequiredUserId();
        var organizationId = GetRequiredOrganizationUserOrganizationId();
        var organization = await GetActiveOrganizationAsync(organizationId);
        await EnsureActiveUserAsync(userId);

        if (!await _subscriptionService.HasActiveSubscriptionAsync(organizationId))
        {
            throw new ValidationException("An active subscription is required before creating an analysis request.");
        }

        var analysisRequest = new AnalysisRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            CreatedByUserId = userId,
            Semester = NormalizeRequired(request.Semester, "Semester is required."),
            Title = NormalizeOptional(request.Title),
            Notes = NormalizeOptional(request.Notes),
            Status = AnalysisRequestStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AnalysisRequests.Add(analysisRequest);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(analysisRequest.Id);
    }

    public async Task<List<AnalysisRequestResponse>> GetMyRequestsAsync()
    {
        var organizationId = GetRequiredOrganizationUserOrganizationId();

        return await QueryResponses()
            .Where(request => request.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<List<AnalysisRequestResponse>> GetAllAsync()
    {
        EnsureAdmin();

        return await QueryResponses().ToListAsync();
    }

    public async Task<AnalysisRequestResponse> GetByIdAsync(Guid id)
    {
        var analysisRequest = await GetAccessibleRequestAsync(id, asTracking: false);

        return MapResponse(analysisRequest);
    }

    public async Task<AnalysisRequestResponse> UpdateStatusAsync(
        Guid id,
        UpdateAnalysisRequestStatusRequest request)
    {
        ValidateRequest(request);
        EnsureAdmin();

        var analysisRequest = await GetRequestEntityAsync(id, asTracking: true);
        var nextStatus = ParseStatus(request.Status);

        EnsureStatusTransitionAllowed(analysisRequest.Status, nextStatus);

        analysisRequest.Status = nextStatus;
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<AnalysisRequestResponse> CancelAsync(Guid id)
    {
        var analysisRequest = await GetAccessibleRequestAsync(id, asTracking: true);

        if (analysisRequest.Status is AnalysisRequestStatus.Sent)
        {
            throw new ValidationException("Sent analysis requests cannot be cancelled.");
        }

        analysisRequest.Status = AnalysisRequestStatus.Cancelled;
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private IQueryable<AnalysisRequest> QueryRequests(bool asTracking)
    {
        var query = _dbContext.AnalysisRequests
            .Include(request => request.Organization)
            .Include(request => request.CreatedByUser)
            .OrderByDescending(request => request.CreatedAt)
            .AsQueryable();

        return asTracking ? query : query.AsNoTracking();
    }

    private IQueryable<AnalysisRequestResponse> QueryResponses()
    {
        return QueryRequests(asTracking: false)
            .Select(request => new AnalysisRequestResponse
            {
                Id = request.Id,
                OrganizationId = request.OrganizationId,
                OrganizationName = request.Organization == null ? string.Empty : request.Organization.Name,
                CreatedByUserId = request.CreatedByUserId,
                CreatedByUserEmail = request.CreatedByUser == null ? string.Empty : request.CreatedByUser.Email ?? string.Empty,
                Semester = request.Semester,
                Title = request.Title,
                Notes = request.Notes,
                Status = request.Status.ToString(),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            });
    }

    private async Task<AnalysisRequest> GetAccessibleRequestAsync(Guid id, bool asTracking)
    {
        var analysisRequest = await GetRequestEntityAsync(id, asTracking);

        if (IsAdmin())
        {
            return analysisRequest;
        }

        var organizationId = GetRequiredOrganizationUserOrganizationId();
        if (analysisRequest.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Analysis request was not found.");
        }

        return analysisRequest;
    }

    private async Task<AnalysisRequest> GetRequestEntityAsync(Guid id, bool asTracking)
    {
        var analysisRequest = await QueryRequests(asTracking)
            .FirstOrDefaultAsync(request => request.Id == id);

        if (analysisRequest is null)
        {
            throw new KeyNotFoundException("Analysis request was not found.");
        }

        return analysisRequest;
    }

    private async Task<Organization> GetActiveOrganizationAsync(Guid organizationId)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(organization => organization.Id == organizationId);

        if (organization is null || !organization.IsActive)
        {
            throw new ValidationException("Organization is inactive or no longer exists.");
        }

        return organization;
    }

    private async Task EnsureActiveUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive or no longer exists.");
        }
    }

    private Guid GetRequiredUserId()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return _currentUserService.UserId.Value;
    }

    private Guid GetRequiredOrganizationUserOrganizationId()
    {
        if (!string.Equals(_currentUserService.Role, ApplicationRoles.OrganizationUser, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Only organization users can perform this action.");
        }

        if (!_currentUserService.OrganizationId.HasValue)
        {
            throw new ValidationException("Organization user is not linked to an organization.");
        }

        return _currentUserService.OrganizationId.Value;
    }

    private void EnsureAdmin()
    {
        if (!IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can perform this action.");
        }
    }

    private bool IsAdmin()
    {
        return string.Equals(_currentUserService.Role, ApplicationRoles.Admin, StringComparison.Ordinal);
    }

    private static AnalysisRequestStatus ParseStatus(string value)
    {
        if (!Enum.TryParse<AnalysisRequestStatus>(value.Trim(), ignoreCase: true, out var status)
            || !Enum.IsDefined(status))
        {
            throw new ValidationException("Status is not valid.");
        }

        return status;
    }

    private static void EnsureStatusTransitionAllowed(
        AnalysisRequestStatus currentStatus,
        AnalysisRequestStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return;
        }

        if (currentStatus is AnalysisRequestStatus.Cancelled)
        {
            throw new ValidationException("Cancelled analysis requests cannot change status.");
        }

        if (currentStatus is AnalysisRequestStatus.Sent)
        {
            throw new ValidationException("Sent analysis requests cannot change status.");
        }

        if (nextStatus is AnalysisRequestStatus.Draft)
        {
            throw new ValidationException("Submitted analysis requests cannot return to draft.");
        }
    }

    private static AnalysisRequestResponse MapResponse(AnalysisRequest request)
    {
        return new AnalysisRequestResponse
        {
            Id = request.Id,
            OrganizationId = request.OrganizationId,
            OrganizationName = request.Organization?.Name ?? string.Empty,
            CreatedByUserId = request.CreatedByUserId,
            CreatedByUserEmail = request.CreatedByUser?.Email ?? string.Empty,
            Semester = request.Semester,
            Title = request.Title,
            Notes = request.Notes,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
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
}
