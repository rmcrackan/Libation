using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class AddLiberatedStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookLocation",
                table: "UserDefinedItem",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookStatus",
                table: "UserDefinedItem",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PdfStatus",
                table: "UserDefinedItem",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookLocation",
                table: "UserDefinedItem");

            migrationBuilder.DropColumn(
                name: "BookStatus",
                table: "UserDefinedItem");

            migrationBuilder.DropColumn(
                name: "PdfStatus",
                table: "UserDefinedItem");
        }
    }
}
