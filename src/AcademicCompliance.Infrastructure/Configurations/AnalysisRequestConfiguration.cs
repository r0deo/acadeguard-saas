using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class AnalysisRequestConfiguration : IEntityTypeConfiguration<AnalysisRequest>
{
    public void Configure(EntityTypeBuilder<AnalysisRequest> builder)
    {
        builder.ToTable("AnalysisRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.OrganizationId)
            .IsRequired();

        builder.Property(request => request.CreatedByUserId)
            .IsRequired();

        builder.Property(request => request.Semester)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(request => request.Title)
            .HasMaxLength(250);

        builder.Property(request => request.Notes)
            .HasMaxLength(1000);

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(request => request.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(request => request.Organization)
            .WithMany()
            .HasForeignKey(request => request.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(request => request.CreatedByUser)
            .WithMany()
            .HasForeignKey(request => request.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(request => request.OrganizationId);

        builder.HasIndex(request => request.CreatedByUserId);

        builder.HasIndex(request => request.Status);

        builder.HasIndex(request => request.CreatedAt);
    }
}
