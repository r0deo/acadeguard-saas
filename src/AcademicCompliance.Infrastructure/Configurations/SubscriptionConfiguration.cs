using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(subscription => subscription.PaymentProvider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(subscription => subscription.PaymentReference)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(subscription => subscription.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(subscription => subscription.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(subscription => subscription.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(subscription => subscription.Organization)
            .WithMany()
            .HasForeignKey(subscription => subscription.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(subscription => subscription.OrganizationId);

        builder.HasIndex(subscription => subscription.PaymentReference)
            .IsUnique();

        builder.HasIndex(subscription => subscription.OrganizationId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'")
            .HasDatabaseName("IX_Subscriptions_OneActivePerOrganization");
    }
}
