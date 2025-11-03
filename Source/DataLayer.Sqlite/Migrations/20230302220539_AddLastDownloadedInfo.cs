using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddLastDownloadedInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastDownloaded",
                table: "UserDefinedItem",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastDownloadedVersion",
                table: "UserDefinedItem",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDownloaded",
                table: "UserDefinedItem");

            migrationBuilder.DropColumn(
                name: "LastDownloadedVersion",
                table: "UserDefinedItem");
        }
    }
}
