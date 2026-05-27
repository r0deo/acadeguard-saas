using AcademicCompliance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicCompliance.Infrastructure.Configurations;

public sealed class MinistryStandardConfiguration : IEntityTypeConfiguration<MinistryStandard>
{
    private static readonly DateTime SeedCreatedAt = new(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<MinistryStandard> builder)
    {
        builder.ToTable("MinistryStandards", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_MinistryStandards_Title",
                "NULLIF(BTRIM(\"TitleArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TitleEnglish\"), '') IS NOT NULL");
        });

        builder.HasKey(standard => standard.Id);

        builder.Property(standard => standard.Number)
            .IsRequired();

        builder.Property(standard => standard.TitleArabic)
            .HasMaxLength(250);

        builder.Property(standard => standard.TitleEnglish)
            .HasMaxLength(250);

        builder.Property(standard => standard.DescriptionArabic)
            .HasMaxLength(2000);

        builder.Property(standard => standard.DescriptionEnglish)
            .HasMaxLength(2000);

        builder.Property(standard => standard.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(standard => standard.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(standard => standard.Number)
            .IsUnique();

        builder.HasMany(standard => standard.Requirements)
            .WithOne(requirement => requirement.Standard)
            .HasForeignKey(requirement => requirement.StandardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(GetSeedStandards());
    }

    private static MinistryStandard[] GetSeedStandards()
    {
        return
        [
            new MinistryStandard
            {
                Id = Guid.Parse("49406d6e-6aac-47db-a4f9-d34125e7652b"),
                Number = 1,
                TitleEnglish = "Governance and Management",
                TitleArabic = "الحوكمة والإدارة",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("d53bef23-3792-4a81-b875-0ca823eb797d"),
                Number = 2,
                TitleEnglish = "Quality Assurance",
                TitleArabic = "ضمان الجودة",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("691969f5-298c-40e8-a162-44b23d8215f7"),
                Number = 3,
                TitleEnglish = "Educational Programs",
                TitleArabic = "البرامج التعليمية",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("1b3c5331-0898-43dd-bdf2-a3cd4a79d58a"),
                Number = 4,
                TitleEnglish = "Research and Scholarly Activities",
                TitleArabic = "الأنشطة البحثية والعلمية",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("7bb49562-72d7-4ad1-b33e-b608dbcd9bcd"),
                Number = 5,
                TitleEnglish = "Faculty and Professional Staff",
                TitleArabic = "هيئة التدريس والموظفون المهنيون",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("a7378a37-8e27-4969-8ceb-ff02313c26c3"),
                Number = 6,
                TitleEnglish = "Students",
                TitleArabic = "الطلاب",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("698cbd07-aace-4259-9b09-93390c35996f"),
                Number = 7,
                TitleEnglish = "Health, Safety and Environment",
                TitleArabic = "الصحة والسلامة والبيئة",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("a598956c-081a-4eb4-9a34-99c0f5f5915b"),
                Number = 8,
                TitleEnglish = "Learning Resource Centre",
                TitleArabic = "مركز مصادر التعلم",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("405da356-8916-4836-a917-e95acd5bd489"),
                Number = 9,
                TitleEnglish = "Fiscal Resources, Financial Management and Budgeting",
                TitleArabic = "الموارد المالية والإدارة المالية والميزانية",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("ded4f5d5-ba92-437a-aa1f-4bf1b833b01d"),
                Number = 10,
                TitleEnglish = "Legal Compliance and Public Disclosure",
                TitleArabic = "الامتثال القانوني والإفصاح العام",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            },
            new MinistryStandard
            {
                Id = Guid.Parse("b301026e-5adb-43c5-921b-bf8753aa6a0d"),
                Number = 11,
                TitleEnglish = "Community Engagement",
                TitleArabic = "المشاركة المجتمعية",
                IsActive = true,
                CreatedAt = SeedCreatedAt
            }
        ];
    }
}
