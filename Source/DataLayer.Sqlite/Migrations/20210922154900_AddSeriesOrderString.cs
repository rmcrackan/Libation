using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

public partial class AddSeriesOrderString : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Index",
			table: "SeriesBook");

		migrationBuilder.AddColumn<string>(
			name: "Order",
			table: "SeriesBook",
			type: "TEXT",
			nullable: true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "Order",
			table: "SeriesBook");

		migrationBuilder.AddColumn<float>(
			name: "Index",
			table: "SeriesBook",
			type: "REAL",
			nullable: true);
	}
}
