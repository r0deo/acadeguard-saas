using AcademicCompliance.Application.DTOs.AnalysisRequests;

namespace AcademicCompliance.Application.Interfaces.AnalysisRequests;

public interface IAnalysisRequestService
{
    Task<AnalysisRequestResponse> CreateAsync(CreateAnalysisRequestRequest request);

    Task<List<AnalysisRequestResponse>> GetMyRequestsAsync();

    Task<List<AnalysisRequestResponse>> GetAllAsync();

    Task<AnalysisRequestResponse> GetByIdAsync(Guid id);

    Task<AnalysisRequestResponse> UpdateStatusAsync(Guid id, UpdateAnalysisRequestStatusRequest request);

    Task<AnalysisRequestResponse> CancelAsync(Guid id);
}
