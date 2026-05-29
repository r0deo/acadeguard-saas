using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicCompliance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Semester = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisRequests_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalysisRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_CreatedAt",
                table: "AnalysisRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_CreatedByUserId",
                table: "AnalysisRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_OrganizationId",
                table: "AnalysisRequests",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_Status",
                table: "AnalysisRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisRequests");
        }
    }
}
