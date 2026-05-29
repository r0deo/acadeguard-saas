using AcademicCompliance.Application.DTOs.Uploads;
using AcademicCompliance.Application.Interfaces.Uploads;
using Microsoft.Extensions.Configuration;

namespace AcademicCompliance.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private const int BufferSize = 81920;

    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredRoot = configuration["FileStorage:RootPath"];
        _rootPath = Path.GetFullPath(
            string.IsNullOrWhiteSpace(configuredRoot) ? "uploads" : configuredRoot);
    }

    public async Task<FileStorageResult> SaveAsync(
        FileStorageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Content is null)
        {
            throw new ArgumentException("File content is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.StoredFileName))
        {
            throw new ArgumentException("Stored file name is required.", nameof(request));
        }

        var relativePath = BuildRelativePath(
            request.OrganizationId,
            request.AnalysisRequestId,
            request.StoredFileName);
        var fullPath = GetSafeFullPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var targetStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
            useAsync: true);

        await request.Content.CopyToAsync(targetStream, cancellationToken);

        return new FileStorageResult
        {
            RelativePath = relativePath
        };
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(relativePath);

        return Task.FromResult(File.Exists(fullPath));
    }

    private static string BuildRelativePath(
        Guid organizationId,
        Guid analysisRequestId,
        string storedFileName)
    {
        var safeStoredFileName = Path.GetFileName(storedFileName);
        if (!string.Equals(safeStoredFileName, storedFileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Stored file name is not valid.", nameof(storedFileName));
        }

        return string.Join(
            '/',
            organizationId.ToString("D"),
            analysisRequestId.ToString("D"),
            safeStoredFileName);
    }

    private string GetSafeFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Relative path is required.", nameof(relativePath));
        }

        var normalizedRelativePath = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedRelativePath))
        {
            throw new ArgumentException("Relative path must not be rooted.", nameof(relativePath));
        }

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalizedRelativePath));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Relative path is outside the upload root.", nameof(relativePath));
        }

        return fullPath;
    }
}
