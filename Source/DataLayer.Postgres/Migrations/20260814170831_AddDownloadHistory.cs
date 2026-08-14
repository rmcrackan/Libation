using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataLayer.Postgres.Migrations
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
                    DownloadHistoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompletedAtUtcTicks = table.Column<long>(type: "bigint", nullable: false),
                    AudibleProductId = table.Column<string>(type: "text", nullable: true),
                    IsAudiblePlus = table.Column<bool>(type: "boolean", nullable: false),
                    Bytes = table.Column<long>(type: "bigint", nullable: false)
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
