using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class UploadedDocumentConfiguration : IEntityTypeConfiguration<UploadedDocument>
{
    public void Configure(EntityTypeBuilder<UploadedDocument> builder)
    {
        builder.ToTable("UploadedDocuments");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.AnalysisRequestId)
            .IsRequired();

        builder.Property(document => document.OrganizationId)
            .IsRequired();

        builder.Property(document => document.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(document => document.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(document => document.RelativePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(document => document.FileExtension)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(document => document.ContentType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(document => document.FileSizeBytes)
            .IsRequired();

        builder.Property(document => document.UploadStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(document => document.UploadedByUserId)
            .IsRequired();

        builder.Property(document => document.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(document => document.AnalysisRequest)
            .WithMany()
            .HasForeignKey(document => document.AnalysisRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(document => document.Organization)
            .WithMany()
            .HasForeignKey(document => document.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(document => document.UploadedByUser)
            .WithMany()
            .HasForeignKey(document => document.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(document => document.AnalysisRequestId);

        builder.HasIndex(document => document.OrganizationId);

        builder.HasIndex(document => document.UploadedByUserId);

        builder.HasIndex(document => document.UploadStatus);

        builder.HasIndex(document => document.CreatedAt);
    }
}
