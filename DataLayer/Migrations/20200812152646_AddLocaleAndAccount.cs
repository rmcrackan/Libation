using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class AddLocaleAndAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Account",
                table: "Library",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Books",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "Library");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Books");
        }
    }
}
