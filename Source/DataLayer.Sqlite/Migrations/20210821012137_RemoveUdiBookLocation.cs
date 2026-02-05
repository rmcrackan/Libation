using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations;

public partial class RemoveUdiBookLocation : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "BookLocation",
			table: "UserDefinedItem");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "BookLocation",
			table: "UserDefinedItem",
			type: "TEXT",
			nullable: true);
	}
}
