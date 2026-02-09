using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

/// <inheritdoc />
public partial class AddBookLanguage : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "Language",
			table: "Books",
			type: "TEXT",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Language",
			table: "Books");
	}
}
