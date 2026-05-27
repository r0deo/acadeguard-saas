using AcademicCompliance.Application.DTOs.Standards;

namespace AcademicCompliance.Application.Interfaces.Standards;

public interface IMinistryStandardsService
{
    Task<List<StandardListItemResponse>> GetStandardsAsync();

    Task<StandardResponse> GetStandardByIdAsync(Guid id);

    Task<StandardTreeResponse> GetStandardTreeAsync(Guid id);

    Task<StandardResponse> UpdateStandardAsync(Guid id, UpdateStandardRequest request);

    Task<RequirementResponse> UpdateRequirementAsync(Guid id, UpdateRequirementRequest request);

    Task<ClauseResponse> UpdateClauseAsync(Guid id, UpdateClauseRequest request);

    Task<BranchResponse> UpdateBranchAsync(Guid id, UpdateBranchRequest request);

    Task ActivateStandardAsync(Guid id);

    Task DeactivateStandardAsync(Guid id);

    Task ActivateRequirementAsync(Guid id);

    Task DeactivateRequirementAsync(Guid id);

    Task ActivateClauseAsync(Guid id);

    Task DeactivateClauseAsync(Guid id);

    Task ActivateBranchAsync(Guid id);

    Task DeactivateBranchAsync(Guid id);
}
