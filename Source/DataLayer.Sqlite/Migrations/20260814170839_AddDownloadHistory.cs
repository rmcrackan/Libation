using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadHistory",
                columns: table => new
                {
                    DownloadHistoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompletedAtUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    AudibleProductId = table.Column<string>(type: "TEXT", nullable: true),
                    IsAudiblePlus = table.Column<bool>(type: "INTEGER", nullable: false),
                    Bytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadHistory", x => x.DownloadHistoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadHistory_CompletedAtUtcTicks",
                table: "DownloadHistory",
                column: "CompletedAtUtcTicks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadHistory");
        }
    }
}
