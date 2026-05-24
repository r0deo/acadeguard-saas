using System.ComponentModel.DataAnnotations;

namespace AcademicCompliance.Application.DTOs.Billing;

public sealed class CreateCheckoutRequest
{
    [Required]
    public Guid OrganizationId { get; init; }
}
