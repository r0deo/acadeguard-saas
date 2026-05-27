using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class MinistryBranchConfiguration : IEntityTypeConfiguration<MinistryBranch>
{
    public void Configure(EntityTypeBuilder<MinistryBranch> builder)
    {
        builder.ToTable("MinistryBranches", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_MinistryBranches_Content",
                "NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");

            tableBuilder.HasCheckConstraint(
                "CK_MinistryBranches_PageNumbers",
                "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
        });

        builder.HasKey(branch => branch.Id);

        builder.Property(branch => branch.ClauseId)
            .IsRequired();

        builder.Property(branch => branch.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(branch => branch.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(branch => branch.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(branch => new { branch.ClauseId, branch.Code })
            .IsUnique();

        builder.HasOne(branch => branch.Clause)
            .WithMany(clause => clause.Branches)
            .HasForeignKey(branch => branch.ClauseId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
