using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

public partial class RenameLibraryBooks : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Library_Books_BookId",
			table: "Library");

		migrationBuilder.DropPrimaryKey(
			name: "PK_Library",
			table: "Library");

		migrationBuilder.RenameTable(
			name: "Library",
			newName: "LibraryBooks");

		migrationBuilder.AddPrimaryKey(
			name: "PK_LibraryBooks",
			table: "LibraryBooks",
			column: "BookId");

		migrationBuilder.AddForeignKey(
			name: "FK_LibraryBooks_Books_BookId",
			table: "LibraryBooks",
			column: "BookId",
			principalTable: "Books",
			principalColumn: "BookId",
			onDelete: ReferentialAction.Cascade);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_LibraryBooks_Books_BookId",
			table: "LibraryBooks");

		migrationBuilder.DropPrimaryKey(
			name: "PK_LibraryBooks",
			table: "LibraryBooks");

		migrationBuilder.RenameTable(
			name: "LibraryBooks",
			newName: "Library");

		migrationBuilder.AddPrimaryKey(
			name: "PK_Library",
			table: "Library",
			column: "BookId");

		migrationBuilder.AddForeignKey(
			name: "FK_Library_Books_BookId",
			table: "Library",
			column: "BookId",
			principalTable: "Books",
			principalColumn: "BookId",
			onDelete: ReferentialAction.Cascade);
	}
}
