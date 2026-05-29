using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicCompliance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedDocuments_AnalysisRequests_AnalysisRequestId",
                        column: x => x.AnalysisRequestId,
                        principalTable: "AnalysisRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedDocuments_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedDocuments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDocuments_AnalysisRequestId",
                table: "UploadedDocuments",
                column: "AnalysisRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDocuments_CreatedAt",
                table: "UploadedDocuments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDocuments_OrganizationId",
                table: "UploadedDocuments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDocuments_UploadedByUserId",
                table: "UploadedDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDocuments_UploadStatus",
                table: "UploadedDocuments",
                column: "UploadStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedDocuments");
        }
    }
}
