using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Books_CategoryId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ParentCategoryCategoryId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Books");

            migrationBuilder.CreateTable(
                name: "CategoryLadders",
                columns: table => new
                {
                    CategoryLadderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryLadders", x => x.CategoryLadderId);
                });

            migrationBuilder.CreateTable(
                name: "BookCategory",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryLadderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategory", x => new { x.BookId, x.CategoryLadderId });
                    table.ForeignKey(
                        name: "FK_BookCategory_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategory_CategoryLadders_CategoryLadderId",
                        column: x => x.CategoryLadderId,
                        principalTable: "CategoryLadders",
                        principalColumn: "CategoryLadderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryCategoryLadder",
                columns: table => new
                {
                    _categoriesCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    _categoryLaddersCategoryLadderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryCategoryLadder", x => new { x._categoriesCategoryId, x._categoryLaddersCategoryLadderId });
                    table.ForeignKey(
                        name: "FK_CategoryCategoryLadder_Categories__categoriesCategoryId",
                        column: x => x._categoriesCategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryCategoryLadder_CategoryLadders__categoryLaddersCategoryLadderId",
                        column: x => x._categoryLaddersCategoryLadderId,
                        principalTable: "CategoryLadders",
                        principalColumn: "CategoryLadderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CategoryLadders",
                column: "CategoryLadderId",
                value: -1);

            migrationBuilder.CreateIndex(
                name: "IX_BookCategory_BookId",
                table: "BookCategory",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategory_CategoryLadderId",
                table: "BookCategory",
                column: "CategoryLadderId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCategoryLadder__categoryLaddersCategoryLadderId",
                table: "CategoryCategoryLadder",
                column: "_categoryLaddersCategoryLadderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCategory");

            migrationBuilder.DropTable(
                name: "CategoryCategoryLadder");

            migrationBuilder.DropTable(
                name: "CategoryLadders");

            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryCategoryId",
                table: "Categories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Books",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: -1,
                column: "ParentCategoryCategoryId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryCategoryId",
                table: "Categories",
                column: "ParentCategoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Categories_CategoryId",
                table: "Books",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryCategoryId",
                table: "Categories",
                column: "ParentCategoryCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId");
        }
    }
}
