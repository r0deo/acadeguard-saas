using System.ComponentModel.DataAnnotations;
using AcademicCompliance.Application.DTOs.Uploads;
using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Application.Interfaces.Auth;
using AcademicCompliance.Application.Interfaces.Billing;
using AcademicCompliance.Application.Interfaces.Uploads;
using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using AcademicCompliance.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Services.Uploads;

public sealed class UploadedDocumentService : IUploadedDocumentService
{
    private const long MaxFileSizeBytes = 50L * 1024L * 1024L;

    private static readonly Dictionary<string, string[]> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".doc"] = ["application/msword"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
    };

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UploadedDocumentService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        ISubscriptionService subscriptionService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _subscriptionService = subscriptionService;
        _userManager = userManager;
    }

    public async Task<UploadDocumentResponse> UploadAsync(
        Guid analysisRequestId,
        UploadDocumentInput file,
        CancellationToken cancellationToken = default)
    {
        var normalizedFile = ValidateAndNormalizeFile(file);
        var userId = GetRequiredUserId();
        var organizationId = GetRequiredOrganizationUserOrganizationId();

        await EnsureActiveUserAsync(userId);
        await EnsureActiveOrganizationAsync(organizationId, cancellationToken);

        if (!await _subscriptionService.HasActiveSubscriptionAsync(organizationId))
        {
            throw new ValidationException("An active subscription is required before uploading documents.");
        }

        var analysisRequest = await GetOrganizationAnalysisRequestAsync(
            analysisRequestId,
            organizationId,
            asTracking: true,
            cancellationToken);

        if (analysisRequest.Status is AnalysisRequestStatus.Cancelled)
        {
            throw new ValidationException("Cancelled analysis requests cannot receive uploaded documents.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{normalizedFile.FileExtension}";
        FileStorageResult? storageResult = null;

        try
        {
            storageResult = await _fileStorageService.SaveAsync(
                new FileStorageRequest
                {
                    OrganizationId = organizationId,
                    AnalysisRequestId = analysisRequestId,
                    StoredFileName = storedFileName,
                    Content = file.Content
                },
                cancellationToken);

            var uploadedDocument = new UploadedDocument
            {
                Id = Guid.NewGuid(),
                AnalysisRequestId = analysisRequestId,
                OrganizationId = organizationId,
                OriginalFileName = normalizedFile.OriginalFileName,
                StoredFileName = storedFileName,
                RelativePath = storageResult.RelativePath,
                FileExtension = normalizedFile.FileExtension,
                ContentType = normalizedFile.ContentType,
                FileSizeBytes = normalizedFile.FileSizeBytes,
                UploadStatus = DocumentUploadStatus.Uploaded,
                UploadedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UploadedDocuments.Add(uploadedDocument);

            if (analysisRequest.Status is AnalysisRequestStatus.Draft or AnalysisRequestStatus.Submitted)
            {
                analysisRequest.Status = AnalysisRequestStatus.FilesUploaded;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapUploadResponse(uploadedDocument);
        }
        catch
        {
            if (storageResult is not null)
            {
                await TryDeleteStoredFileAsync(storageResult.RelativePath, cancellationToken);
            }

            throw;
        }
    }

    public async Task<List<UploadedDocumentResponse>> GetRequestDocumentsAsync(
        Guid analysisRequestId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRequestAccessibleAsync(analysisRequestId, cancellationToken);

        return await QueryDocumentEntities(asTracking: false)
            .Where(document => document.AnalysisRequestId == analysisRequestId)
            .Select(document => new UploadedDocumentResponse
            {
                Id = document.Id,
                OriginalFileName = document.OriginalFileName,
                FileExtension = document.FileExtension,
                FileSizeBytes = document.FileSizeBytes,
                UploadStatus = document.UploadStatus.ToString(),
                UploadedAt = document.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UploadedDocumentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        return await QueryDocuments().ToListAsync(cancellationToken);
    }

    public async Task<UploadedDocumentResponse> GetDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await GetAccessibleDocumentAsync(documentId, asTracking: false, cancellationToken);

        return MapDocumentResponse(document);
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetAccessibleDocumentAsync(documentId, asTracking: true, cancellationToken);

        await _fileStorageService.DeleteAsync(document.RelativePath, cancellationToken);

        _dbContext.UploadedDocuments.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<UploadedDocument> QueryDocumentEntities(bool asTracking)
    {
        var query = _dbContext.UploadedDocuments
            .Include(document => document.AnalysisRequest)
            .OrderByDescending(document => document.CreatedAt)
            .AsQueryable();

        return asTracking ? query : query.AsNoTracking();
    }

    private IQueryable<UploadedDocumentResponse> QueryDocuments()
    {
        return QueryDocumentEntities(asTracking: false)
            .Select(document => new UploadedDocumentResponse
            {
                Id = document.Id,
                OriginalFileName = document.OriginalFileName,
                FileExtension = document.FileExtension,
                FileSizeBytes = document.FileSizeBytes,
                UploadStatus = document.UploadStatus.ToString(),
                UploadedAt = document.CreatedAt
            });
    }

    private async Task<UploadedDocument> GetAccessibleDocumentAsync(
        Guid documentId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var document = await QueryDocumentEntities(asTracking)
            .FirstOrDefaultAsync(document => document.Id == documentId, cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException("Uploaded document was not found.");
        }

        if (IsAdmin())
        {
            return document;
        }

        var organizationId = GetRequiredOrganizationUserOrganizationId();
        if (document.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Uploaded document was not found.");
        }

        return document;
    }

    private async Task EnsureRequestAccessibleAsync(Guid analysisRequestId, CancellationToken cancellationToken)
    {
        if (IsAdmin())
        {
            var exists = await _dbContext.AnalysisRequests
                .AsNoTracking()
                .AnyAsync(request => request.Id == analysisRequestId, cancellationToken);

            if (!exists)
            {
                throw new KeyNotFoundException("Analysis request was not found.");
            }

            return;
        }

        var organizationId = GetRequiredOrganizationUserOrganizationId();
        _ = await GetOrganizationAnalysisRequestAsync(
            analysisRequestId,
            organizationId,
            asTracking: false,
            cancellationToken);
    }

    private async Task<AnalysisRequest> GetOrganizationAnalysisRequestAsync(
        Guid analysisRequestId,
        Guid organizationId,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AnalysisRequests.AsQueryable();
        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        var analysisRequest = await query
            .FirstOrDefaultAsync(request => request.Id == analysisRequestId, cancellationToken);

        if (analysisRequest is null || analysisRequest.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Analysis request was not found.");
        }

        return analysisRequest;
    }

    private async Task EnsureActiveOrganizationAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organizationIsActive = await _dbContext.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization => organization.Id == organizationId && organization.IsActive,
                cancellationToken);

        if (!organizationIsActive)
        {
            throw new ValidationException("Organization is inactive or no longer exists.");
        }
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

    private static NormalizedUploadFile ValidateAndNormalizeFile(UploadDocumentInput file)
    {
        if (file is null)
        {
            throw new ValidationException("File is required.");
        }

        if (file.Content is null || file.Content == Stream.Null)
        {
            throw new ValidationException("File content is required.");
        }

        if (file.FileSizeBytes <= 0)
        {
            throw new ValidationException("File cannot be empty.");
        }

        if (file.FileSizeBytes > MaxFileSizeBytes)
        {
            throw new ValidationException("File size cannot exceed 50 MB.");
        }

        var originalFileName = NormalizeOriginalFileName(file.OriginalFileName);
        var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!AllowedContentTypes.TryGetValue(fileExtension, out var allowedContentTypes))
        {
            throw new ValidationException("Only PDF, DOC, and DOCX files are allowed.");
        }

        var contentType = file.ContentType.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(contentType)
            || !allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException("File content type is not allowed.");
        }

        return new NormalizedUploadFile(
            originalFileName,
            fileExtension,
            contentType,
            file.FileSizeBytes);
    }

    private static string NormalizeOriginalFileName(string value)
    {
        var fileName = Path.GetFileName(value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ValidationException("Original file name is required.");
        }

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '_');
        }

        if (fileName.Length <= 255)
        {
            return fileName;
        }

        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var maximumNameLength = 255 - extension.Length;

        return nameWithoutExtension[..maximumNameLength] + extension;
    }

    private async Task TryDeleteStoredFileAsync(string relativePath, CancellationToken cancellationToken)
    {
        try
        {
            await _fileStorageService.DeleteAsync(relativePath, cancellationToken);
        }
        catch
        {
            // Best-effort cleanup after a failed metadata save.
        }
    }

    private static UploadDocumentResponse MapUploadResponse(UploadedDocument document)
    {
        return new UploadDocumentResponse
        {
            DocumentId = document.Id,
            OriginalFileName = document.OriginalFileName,
            FileSizeBytes = document.FileSizeBytes,
            UploadStatus = document.UploadStatus.ToString(),
            UploadedAt = document.CreatedAt
        };
    }

    private static UploadedDocumentResponse MapDocumentResponse(UploadedDocument document)
    {
        return new UploadedDocumentResponse
        {
            Id = document.Id,
            OriginalFileName = document.OriginalFileName,
            FileExtension = document.FileExtension,
            FileSizeBytes = document.FileSizeBytes,
            UploadStatus = document.UploadStatus.ToString(),
            UploadedAt = document.CreatedAt
        };
    }

    private sealed record NormalizedUploadFile(
        string OriginalFileName,
        string FileExtension,
        string ContentType,
        long FileSizeBytes);
}
