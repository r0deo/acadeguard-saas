using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Enums;

namespace AcademicCompliance.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

    public PaymentProvider PaymentProvider { get; set; }

    public string PaymentReference { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;
}
