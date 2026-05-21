using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(organization => organization.Id);

        builder.Property(organization => organization.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(organization => organization.OfficialEmail)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(organization => organization.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(organization => organization.ContactPersonName)
            .HasMaxLength(200);

        builder.Property(organization => organization.ContactPersonEmail)
            .HasMaxLength(250);

        builder.Property(organization => organization.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(organization => organization.Address)
            .HasMaxLength(500);

        builder.Property(organization => organization.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(organization => organization.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(organization => organization.Name)
            .IsUnique();

        builder.HasIndex(organization => organization.OfficialEmail)
            .IsUnique();
    }
}
