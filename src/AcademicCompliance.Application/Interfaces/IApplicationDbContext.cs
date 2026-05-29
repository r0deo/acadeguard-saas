using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<Subscription> Subscriptions { get; }

    DbSet<Payment> Payments { get; }

    DbSet<MinistryStandard> MinistryStandards { get; }

    DbSet<MinistryRequirement> MinistryRequirements { get; }

    DbSet<MinistryClause> MinistryClauses { get; }

    DbSet<MinistryBranch> MinistryBranches { get; }

    DbSet<AnalysisRequest> AnalysisRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
