using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataLayer.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AudibleProductId = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Subtitle = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LengthInMinutes = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<string>(type: "text", nullable: true),
                    PictureId = table.Column<string>(type: "text", nullable: true),
                    PictureLarge = table.Column<string>(type: "text", nullable: true),
                    IsAbridged = table.Column<bool>(type: "boolean", nullable: false),
                    IsSpatial = table.Column<bool>(type: "boolean", nullable: false),
                    DatePublished = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Rating_OverallRating = table.Column<float>(type: "real", nullable: true),
                    Rating_PerformanceRating = table.Column<float>(type: "real", nullable: true),
                    Rating_StoryRating = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AudibleCategoryId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CategoryLadders",
                columns: table => new
                {
                    CategoryLadderId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryLadders", x => x.CategoryLadderId);
                });

            migrationBuilder.CreateTable(
                name: "Contributors",
                columns: table => new
                {
                    ContributorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    AudibleContributorId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributors", x => x.ContributorId);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AudibleSeriesId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.SeriesId);
                });

            migrationBuilder.CreateTable(
                name: "LibraryBooks",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Account = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    AbsentFromLastScan = table.Column<bool>(type: "boolean", nullable: false),
                    IncludedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBooks", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_LibraryBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supplement",
                columns: table => new
                {
                    SupplementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true)
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
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    LastDownloaded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastDownloadedVersion = table.Column<string>(type: "text", nullable: true),
                    LastDownloadedFormat = table.Column<long>(type: "bigint", nullable: true),
                    LastDownloadedFileVersion = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    Rating_OverallRating = table.Column<float>(type: "real", nullable: true),
                    Rating_PerformanceRating = table.Column<float>(type: "real", nullable: true),
                    Rating_StoryRating = table.Column<float>(type: "real", nullable: true),
                    BookStatus = table.Column<int>(type: "integer", nullable: false),
                    PdfStatus = table.Column<int>(type: "integer", nullable: true),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "BookCategory",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    CategoryLadderId = table.Column<int>(type: "integer", nullable: false)
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
                    _categoriesCategoryId = table.Column<int>(type: "integer", nullable: false),
                    _categoryLaddersCategoryLadderId = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_CategoryCategoryLadder_CategoryLadders__categoryLaddersCate~",
                        column: x => x._categoryLaddersCategoryLadderId,
                        principalTable: "CategoryLadders",
                        principalColumn: "CategoryLadderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookContributor",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    ContributorId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<byte>(type: "smallint", nullable: false)
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
                name: "SeriesBook",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.InsertData(
                table: "Contributors",
                columns: new[] { "ContributorId", "AudibleContributorId", "Name" },
                values: new object[] { -1, null, "" });

            migrationBuilder.CreateIndex(
                name: "IX_BookCategory_BookId",
                table: "BookCategory",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategory_CategoryLadderId",
                table: "BookCategory",
                column: "CategoryLadderId");

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
                name: "IX_Categories_AudibleCategoryId",
                table: "Categories",
                column: "AudibleCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCategoryLadder__categoryLaddersCategoryLadderId",
                table: "CategoryCategoryLadder",
                column: "_categoryLaddersCategoryLadderId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCategory");

            migrationBuilder.DropTable(
                name: "BookContributor");

            migrationBuilder.DropTable(
                name: "CategoryCategoryLadder");

            migrationBuilder.DropTable(
                name: "LibraryBooks");

            migrationBuilder.DropTable(
                name: "SeriesBook");

            migrationBuilder.DropTable(
                name: "Supplement");

            migrationBuilder.DropTable(
                name: "UserDefinedItem");

            migrationBuilder.DropTable(
                name: "Contributors");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "CategoryLadders");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
