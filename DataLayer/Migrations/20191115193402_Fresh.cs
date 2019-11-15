using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class Fresh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudibleCategoryId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ParentCategoryCategoryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryCategoryId",
                        column: x => x.ParentCategoryCategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contributors",
                columns: table => new
                {
                    ContributorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    AudibleAuthorId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributors", x => x.ContributorId);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    SeriesId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudibleSeriesId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.SeriesId);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AudibleProductId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    LengthInMinutes = table.Column<int>(nullable: false),
                    PictureId = table.Column<string>(nullable: true),
                    IsAbridged = table.Column<bool>(nullable: false),
                    DatePublished = table.Column<DateTime>(nullable: true),
                    CategoryId = table.Column<int>(nullable: false),
                    Rating_OverallRating = table.Column<float>(nullable: true),
                    Rating_PerformanceRating = table.Column<float>(nullable: true),
                    Rating_StoryRating = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_Books_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookContributor",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    ContributorId = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    Order = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookContributor", x => new { x.BookId, x.ContributorId, x.Role });
                    table.ForeignKey(
                        name: "FK_BookContributor_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookContributor_Contributors_ContributorId",
                        column: x => x.ContributorId,
                        principalTable: "Contributors",
                        principalColumn: "ContributorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Library",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    DateAdded = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Library", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_Library_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeriesBook",
                columns: table => new
                {
                    SeriesId = table.Column<int>(nullable: false),
                    BookId = table.Column<int>(nullable: false),
                    Index = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesBook", x => new { x.SeriesId, x.BookId });
                    table.ForeignKey(
                        name: "FK_SeriesBook_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeriesBook_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supplement",
                columns: table => new
                {
                    SupplementId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplement", x => x.SupplementId);
                    table.ForeignKey(
                        name: "FK_Supplement_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDefinedItem",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: true),
                    Rating_OverallRating = table.Column<float>(nullable: true),
                    Rating_PerformanceRating = table.Column<float>(nullable: true),
                    Rating_StoryRating = table.Column<float>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefinedItem", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_UserDefinedItem_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "AudibleCategoryId", "Name", "ParentCategoryCategoryId" },
                values: new object[] { -1, "", "", null });

            migrationBuilder.CreateIndex(
                name: "IX_BookContributor_BookId",
                table: "BookContributor",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookContributor_ContributorId",
                table: "BookContributor",
                column: "ContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_AudibleProductId",
                table: "Books",
                column: "AudibleProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_AudibleCategoryId",
                table: "Categories",
                column: "AudibleCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryCategoryId",
                table: "Categories",
                column: "ParentCategoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_Name",
                table: "Contributors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Series_AudibleSeriesId",
                table: "Series",
                column: "AudibleSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesBook_BookId",
                table: "SeriesBook",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesBook_SeriesId",
                table: "SeriesBook",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplement_BookId",
                table: "Supplement",
                column: "BookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookContributor");

            migrationBuilder.DropTable(
                name: "Library");

            migrationBuilder.DropTable(
                name: "SeriesBook");

            migrationBuilder.DropTable(
                name: "Supplement");

            migrationBuilder.DropTable(
                name: "UserDefinedItem");

            migrationBuilder.DropTable(
                name: "Contributors");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
