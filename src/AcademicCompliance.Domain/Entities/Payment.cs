using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Enums;

namespace AcademicCompliance.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrganizationId { get; set; }

    public Organization? Organization { get; set; }

    public Guid? SubscriptionId { get; set; }

    public Subscription? Subscription { get; set; }

    public PaymentProvider Provider { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime? PaidAt { get; set; }
}
