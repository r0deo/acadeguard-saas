using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicCompliance.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<Subscription> Subscriptions { get; }

    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
