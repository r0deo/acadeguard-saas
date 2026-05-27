using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class MinistryRequirementConfiguration : IEntityTypeConfiguration<MinistryRequirement>
{
    public void Configure(EntityTypeBuilder<MinistryRequirement> builder)
    {
        builder.ToTable("MinistryRequirements", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_MinistryRequirements_Content",
                "NULLIF(BTRIM(\"TitleArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TitleEnglish\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");

            tableBuilder.HasCheckConstraint(
                "CK_MinistryRequirements_PageNumbers",
                "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
        });

        builder.HasKey(requirement => requirement.Id);

        builder.Property(requirement => requirement.StandardId)
            .IsRequired();

        builder.Property(requirement => requirement.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(requirement => requirement.TitleArabic)
            .HasMaxLength(500);

        builder.Property(requirement => requirement.TitleEnglish)
            .HasMaxLength(500);

        builder.Property(requirement => requirement.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(requirement => requirement.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(requirement => new { requirement.StandardId, requirement.Code })
            .IsUnique();

        builder.HasOne(requirement => requirement.Standard)
            .WithMany(standard => standard.Requirements)
            .HasForeignKey(requirement => requirement.StandardId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(requirement => requirement.Clauses)
            .WithOne(clause => clause.Requirement)
            .HasForeignKey(clause => clause.RequirementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
