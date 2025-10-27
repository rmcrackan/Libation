using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioFormatData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "_audioFormat",
                table: "Books",
                newName: "IsSpatial");

            migrationBuilder.AddColumn<string>(
                name: "LastDownloadedFileVersion",
                table: "UserDefinedItem",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastDownloadedFormat",
                table: "UserDefinedItem",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDownloadedFileVersion",
                table: "UserDefinedItem");

            migrationBuilder.DropColumn(
                name: "LastDownloadedFormat",
                table: "UserDefinedItem");

            migrationBuilder.RenameColumn(
                name: "IsSpatial",
                table: "Books",
                newName: "_audioFormat");
        }
    }
}
