using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddBookCopyright : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Copyright",
                table: "Books",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Copyright",
                table: "Books");
        }
    }
}
