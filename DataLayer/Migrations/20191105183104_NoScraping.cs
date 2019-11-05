using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class NoScraping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supplement_Books_BookId",
                table: "Supplement");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDefinedItem_Books_BookId",
                table: "UserDefinedItem");

            migrationBuilder.DropColumn(
                name: "DownloadBookLink",
                table: "Library");

            migrationBuilder.DropColumn(
                name: "HasBookDetails",
                table: "Books");

            migrationBuilder.AddForeignKey(
                name: "FK_Supplement_Books_BookId",
                table: "Supplement",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDefinedItem_Books_BookId",
                table: "UserDefinedItem",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Supplement_Books_BookId",
                table: "Supplement");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDefinedItem_Books_BookId",
                table: "UserDefinedItem");

            migrationBuilder.AddColumn<string>(
                name: "DownloadBookLink",
                table: "Library",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBookDetails",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Supplement_Books_BookId",
                table: "Supplement",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDefinedItem_Books_BookId",
                table: "UserDefinedItem",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
