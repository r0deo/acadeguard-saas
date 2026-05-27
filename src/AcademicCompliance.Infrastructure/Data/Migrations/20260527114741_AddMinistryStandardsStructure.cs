using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AcademicCompliance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMinistryStandardsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinistryStandards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    TitleArabic = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TitleEnglish = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DescriptionArabic = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DescriptionEnglish = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryStandards", x => x.Id);
                    table.CheckConstraint("CK_MinistryStandards_Title", "NULLIF(BTRIM(\"TitleArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TitleEnglish\"), '') IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "MinistryRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TitleArabic = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TitleEnglish = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TextArabic = table.Column<string>(type: "text", nullable: true),
                    TextEnglish = table.Column<string>(type: "text", nullable: true),
                    PageNumberArabic = table.Column<int>(type: "integer", nullable: true),
                    PageNumberEnglish = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryRequirements", x => x.Id);
                    table.CheckConstraint("CK_MinistryRequirements_Content", "NULLIF(BTRIM(\"TitleArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TitleEnglish\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");
                    table.CheckConstraint("CK_MinistryRequirements_PageNumbers", "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
                    table.ForeignKey(
                        name: "FK_MinistryRequirements_MinistryStandards_StandardId",
                        column: x => x.StandardId,
                        principalTable: "MinistryStandards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MinistryClauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TextArabic = table.Column<string>(type: "text", nullable: true),
                    TextEnglish = table.Column<string>(type: "text", nullable: true),
                    PageNumberArabic = table.Column<int>(type: "integer", nullable: true),
                    PageNumberEnglish = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryClauses", x => x.Id);
                    table.CheckConstraint("CK_MinistryClauses_Content", "NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");
                    table.CheckConstraint("CK_MinistryClauses_PageNumbers", "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
                    table.ForeignKey(
                        name: "FK_MinistryClauses_MinistryRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "MinistryRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MinistryBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClauseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TextArabic = table.Column<string>(type: "text", nullable: true),
                    TextEnglish = table.Column<string>(type: "text", nullable: true),
                    PageNumberArabic = table.Column<int>(type: "integer", nullable: true),
                    PageNumberEnglish = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryBranches", x => x.Id);
                    table.CheckConstraint("CK_MinistryBranches_Content", "NULLIF(BTRIM(\"TextArabic\"), '') IS NOT NULL OR NULLIF(BTRIM(\"TextEnglish\"), '') IS NOT NULL");
                    table.CheckConstraint("CK_MinistryBranches_PageNumbers", "(\"PageNumberArabic\" IS NULL OR \"PageNumberArabic\" > 0) AND (\"PageNumberEnglish\" IS NULL OR \"PageNumberEnglish\" > 0)");
                    table.ForeignKey(
                        name: "FK_MinistryBranches_MinistryClauses_ClauseId",
                        column: x => x.ClauseId,
                        principalTable: "MinistryClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MinistryStandards",
                columns: new[] { "Id", "CreatedAt", "DescriptionArabic", "DescriptionEnglish", "IsActive", "Number", "TitleArabic", "TitleEnglish", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1b3c5331-0898-43dd-bdf2-a3cd4a79d58a"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 4, "الأنشطة البحثية والعلمية", "Research and Scholarly Activities", null },
                    { new Guid("405da356-8916-4836-a917-e95acd5bd489"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 9, "الموارد المالية والإدارة المالية والميزانية", "Fiscal Resources, Financial Management and Budgeting", null },
                    { new Guid("49406d6e-6aac-47db-a4f9-d34125e7652b"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 1, "الحوكمة والإدارة", "Governance and Management", null },
                    { new Guid("691969f5-298c-40e8-a162-44b23d8215f7"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 3, "البرامج التعليمية", "Educational Programs", null },
                    { new Guid("698cbd07-aace-4259-9b09-93390c35996f"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 7, "الصحة والسلامة والبيئة", "Health, Safety and Environment", null },
                    { new Guid("7bb49562-72d7-4ad1-b33e-b608dbcd9bcd"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 5, "هيئة التدريس والموظفون المهنيون", "Faculty and Professional Staff", null },
                    { new Guid("a598956c-081a-4eb4-9a34-99c0f5f5915b"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 8, "مركز مصادر التعلم", "Learning Resource Centre", null },
                    { new Guid("a7378a37-8e27-4969-8ceb-ff02313c26c3"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 6, "الطلاب", "Students", null },
                    { new Guid("b301026e-5adb-43c5-921b-bf8753aa6a0d"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 11, "المشاركة المجتمعية", "Community Engagement", null },
                    { new Guid("d53bef23-3792-4a81-b875-0ca823eb797d"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 2, "ضمان الجودة", "Quality Assurance", null },
                    { new Guid("ded4f5d5-ba92-437a-aa1f-4bf1b833b01d"), new DateTime(2026, 5, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 10, "الامتثال القانوني والإفصاح العام", "Legal Compliance and Public Disclosure", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinistryBranches_ClauseId_Code",
                table: "MinistryBranches",
                columns: new[] { "ClauseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinistryClauses_RequirementId_Code",
                table: "MinistryClauses",
                columns: new[] { "RequirementId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinistryRequirements_StandardId_Code",
                table: "MinistryRequirements",
                columns: new[] { "StandardId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MinistryStandards_Number",
                table: "MinistryStandards",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinistryBranches");

            migrationBuilder.DropTable(
                name: "MinistryClauses");

            migrationBuilder.DropTable(
                name: "MinistryRequirements");

            migrationBuilder.DropTable(
                name: "MinistryStandards");
        }
    }
}
