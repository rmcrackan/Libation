using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataLayer.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadAttemptFailures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadAttemptFailures",
                columns: table => new
                {
                    DownloadAttemptFailureId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AudibleProductId = table.Column<string>(type: "text", nullable: false),
                    Account = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    LastFailedAtUtcTicks = table.Column<long>(type: "bigint", nullable: false),
                    RetryAfterUtcTicks = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadAttemptFailures", x => x.DownloadAttemptFailureId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadAttemptFailures_Account_AudibleProductId",
                table: "DownloadAttemptFailures",
                columns: new[] { "Account", "AudibleProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadAttemptFailures");
        }
    }
}
