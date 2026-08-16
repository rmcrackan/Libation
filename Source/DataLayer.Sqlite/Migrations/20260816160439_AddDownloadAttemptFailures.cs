using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
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
                    DownloadAttemptFailureId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AudibleProductId = table.Column<string>(type: "TEXT", nullable: false),
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "INTEGER", nullable: false),
                    LastFailedAtUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    RetryAfterUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
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
