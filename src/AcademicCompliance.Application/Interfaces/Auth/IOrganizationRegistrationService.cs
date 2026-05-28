using AcademicCompliance.Application.DTOs.Auth;

namespace AcademicCompliance.Application.Interfaces.Auth;

public interface IOrganizationRegistrationService
{
    Task<RegisterOrganizationResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request);
}
