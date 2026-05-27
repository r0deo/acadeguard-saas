using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class MinistryClauseConfiguration : IEntityTypeConfiguration<MinistryClause>
{
    public void Configure(EntityTypeBuilder<MinistryClause> builder)
    {
        builder.ToTable("MinistryClauses", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_MinistryClauses_Content",
                "NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");

            tableBuilder.HasCheckConstraint(
                "CK_MinistryClauses_PageNumbers",
                "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
        });

        builder.HasKey(clause => clause.Id);

        builder.Property(clause => clause.RequirementId)
            .IsRequired();

        builder.Property(clause => clause.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(clause => clause.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(clause => clause.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(clause => new { clause.RequirementId, clause.Code })
            .IsUnique();

        builder.HasOne(clause => clause.Requirement)
            .WithMany(requirement => requirement.Clauses)
            .HasForeignKey(clause => clause.RequirementId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(clause => clause.Branches)
            .WithOne(branch => branch.Clause)
            .HasForeignKey(branch => branch.ClauseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
