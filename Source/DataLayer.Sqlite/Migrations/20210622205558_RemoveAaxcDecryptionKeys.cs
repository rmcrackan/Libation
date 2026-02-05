using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

public partial class RemoveAaxcDecryptionKeys : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "AudibleIV",
			table: "Books");

		migrationBuilder.DropColumn(
			name: "AudibleKey",
			table: "Books");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "AudibleIV",
			table: "Books",
			type: "TEXT",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "AudibleKey",
			table: "Books",
			type: "TEXT",
			nullable: true);
	}
}
