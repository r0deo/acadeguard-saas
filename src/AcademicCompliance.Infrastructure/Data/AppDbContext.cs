using AcademicCompliance.Application.Interfaces;
using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentEntity = AcademicCompliance.Domain.Entities.Payment;

namespace AcademicCompliance.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    public DbSet<MinistryStandard> MinistryStandards => Set<MinistryStandard>();

    public DbSet<MinistryRequirement> MinistryRequirements => Set<MinistryRequirement>();

    public DbSet<MinistryClause> MinistryClauses => Set<MinistryClause>();

    public DbSet<MinistryBranch> MinistryBranches => Set<MinistryBranch>();

    public DbSet<AnalysisRequest> AnalysisRequests => Set<AnalysisRequest>();

    public DbSet<UploadedDocument> UploadedDocuments => Set<UploadedDocument>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityTimestamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.Property(user => user.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(user => user.Email)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(user => user.NormalizedEmail)
                .HasMaxLength(256)
                .IsRequired();

            builder.HasIndex(user => user.NormalizedEmail)
                .HasDatabaseName("EmailIndex")
                .IsUnique();

            builder.Property(user => user.IsActive)
                .HasDefaultValue(true);

            builder.Property(user => user.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(user => user.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        if (HasEntityTypeConfigurations())
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }

    private void ApplyEntityTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.Id == Guid.Empty)
                {
                    entry.Entity.Id = Guid.NewGuid();
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }

    private static bool HasEntityTypeConfigurations()
    {
        return typeof(AppDbContext).Assembly
            .GetTypes()
            .Any(type => !type.IsAbstract
                && type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));
    }
}
