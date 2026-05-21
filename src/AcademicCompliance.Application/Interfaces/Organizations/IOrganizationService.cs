using AcademicCompliance.Application.DTOs.Organizations;

namespace AcademicCompliance.Application.Interfaces.Organizations;

public interface IOrganizationService
{
    Task<List<OrganizationListItemResponse>> GetAllAsync(string? search, bool? isActive);

    Task<OrganizationResponse> GetByIdAsync(Guid id);

    Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request);

    Task<OrganizationResponse> UpdateAsync(Guid id, UpdateOrganizationRequest request);

    Task ActivateAsync(Guid id);

    Task DeactivateAsync(Guid id);
}
