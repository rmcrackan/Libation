using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

/// <inheritdoc />
public partial class AddIsAudiblePlus : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "Tags",
			table: "UserDefinedItem",
			type: "TEXT",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "TEXT",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_StoryRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "REAL",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_PerformanceRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "REAL",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_OverallRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "REAL",
			oldNullable: true);

		migrationBuilder.AddColumn<bool>(
			name: "IsAudiblePlus",
			table: "LibraryBooks",
			type: "INTEGER",
			nullable: false,
			defaultValue: false);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "IsAudiblePlus",
			table: "LibraryBooks");

		migrationBuilder.AlterColumn<string>(
			name: "Tags",
			table: "UserDefinedItem",
			type: "TEXT",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "TEXT");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_StoryRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "REAL");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_PerformanceRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "REAL");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_OverallRating",
			table: "UserDefinedItem",
			type: "REAL",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "REAL");
	}
}
