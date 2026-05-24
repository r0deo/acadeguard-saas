using AcademicCompliance.Domain.Entities;
using AcademicCompliance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentEntity = AcademicCompliance.Domain.Entities.Payment;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Provider)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(payment => payment.TransactionId)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(payment => payment.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(payment => payment.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(payment => payment.Organization)
            .WithMany()
            .HasForeignKey(payment => payment.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(payment => payment.Subscription)
            .WithMany()
            .HasForeignKey(payment => payment.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(payment => payment.OrganizationId);

        builder.HasIndex(payment => payment.SubscriptionId);

        builder.HasIndex(payment => new { payment.Provider, payment.TransactionId })
            .IsUnique();
    }
}
