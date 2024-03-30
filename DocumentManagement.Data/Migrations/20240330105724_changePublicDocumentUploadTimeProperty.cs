using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Data.Migrations
{
    public partial class changePublicDocumentUploadTimeProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPublicLinkToken",
                table: "Documents");

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicDocumentUploadTime",
                table: "Documents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicDocumentUploadTime",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "DocumentPublicLinkToken",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
