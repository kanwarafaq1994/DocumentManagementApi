using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Data.Migrations
{
    public partial class Initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    EditedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditedBy = table.Column<int>(type: "int", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocymentPublicLinkToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "EditedBy", "EditedOn", "Email", "FailedLoginCount", "FirstName", "IsActive", "IsAdmin", "LastActivity", "LastName", "PasswordHash" },
                values: new object[] { 1, 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "test@test.de", 0, "Kanwar", true, true, null, "Afaq", "$2a$10$TWb.9jrYBS5psxE7mlMCeeBS44tdDXUntu.RVAKlbu8BZCwMmv.ly" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "EditedBy", "EditedOn", "Email", "FailedLoginCount", "FirstName", "IsActive", "IsAdmin", "LastActivity", "LastName", "PasswordHash" },
                values: new object[] { 2, 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "test1@test.de", 0, "Test", true, false, null, "User1", "$2a$10$TWb.9jrYBS5psxE7mlMCeeBS44tdDXUntu.RVAKlbu8BZCwMmv.ly" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "EditedBy", "EditedOn", "Email", "FailedLoginCount", "FirstName", "IsActive", "IsAdmin", "LastActivity", "LastName", "PasswordHash" },
                values: new object[] { 3, 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "test2@test.de", 0, "Test", true, false, null, "User2", "$2a$10$TWb.9jrYBS5psxE7mlMCeeBS44tdDXUntu.RVAKlbu8BZCwMmv.ly" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
