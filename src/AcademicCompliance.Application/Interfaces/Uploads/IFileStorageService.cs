using AcademicCompliance.Application.DTOs.Uploads;

namespace AcademicCompliance.Application.Interfaces.Uploads;

public interface IFileStorageService
{
    Task<FileStorageResult> SaveAsync(
        FileStorageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
