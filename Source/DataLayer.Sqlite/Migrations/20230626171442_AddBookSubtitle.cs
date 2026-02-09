using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

/// <inheritdoc />
public partial class AddBookSubtitle : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "Subtitle",
			table: "Books",
			type: "TEXT",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Subtitle",
			table: "Books");
	}
}
