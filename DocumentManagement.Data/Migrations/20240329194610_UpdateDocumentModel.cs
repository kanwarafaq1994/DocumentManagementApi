using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentManagement.Data.Migrations
{
    public partial class UpdateDocumentModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocymentPublicLinkToken",
                table: "Documents",
                newName: "DocumentPublicLinkToken");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDownloads",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfDownloads",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "DocumentPublicLinkToken",
                table: "Documents",
                newName: "DocymentPublicLinkToken");
        }
    }
}
