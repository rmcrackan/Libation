using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Postgres.Migrations;

/// <inheritdoc />
public partial class MakeDbNullable : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Name",
			table: "Categories");

		migrationBuilder.AlterColumn<string>(
			name: "Url",
			table: "Supplement",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "AudibleSeriesId",
			table: "Series",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Account",
			table: "LibraryBooks",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Name",
			table: "Contributors",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "AudibleCategoryId",
			table: "Categories",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Title",
			table: "Books",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Subtitle",
			table: "Books",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_StoryRating",
			table: "Books",
			type: "real",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "real",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_PerformanceRating",
			table: "Books",
			type: "real",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "real",
			oldNullable: true);

		migrationBuilder.AlterColumn<float>(
			name: "Rating_OverallRating",
			table: "Books",
			type: "real",
			nullable: false,
			defaultValue: 0f,
			oldClrType: typeof(float),
			oldType: "real",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Locale",
			table: "Books",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Description",
			table: "Books",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "AudibleProductId",
			table: "Books",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "Url",
			table: "Supplement",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "AudibleSeriesId",
			table: "Series",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "Account",
			table: "LibraryBooks",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "Name",
			table: "Contributors",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "AudibleCategoryId",
			table: "Categories",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AddColumn<string>(
			name: "Name",
			table: "Categories",
			type: "text",
			nullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "Title",
			table: "Books",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "Subtitle",
			table: "Books",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_StoryRating",
			table: "Books",
			type: "real",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "real");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_PerformanceRating",
			table: "Books",
			type: "real",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "real");

		migrationBuilder.AlterColumn<float>(
			name: "Rating_OverallRating",
			table: "Books",
			type: "real",
			nullable: true,
			oldClrType: typeof(float),
			oldType: "real");

		migrationBuilder.AlterColumn<string>(
			name: "Locale",
			table: "Books",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "Description",
			table: "Books",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "AudibleProductId",
			table: "Books",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");
	}
}
