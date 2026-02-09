using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

/// <inheritdoc />
public partial class MyComment : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
			name: "IsFinished",
			table: "UserDefinedItem",
			type: "INTEGER",
			nullable: false,
			defaultValue: false);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "IsFinished",
			table: "UserDefinedItem");
	}
}
