using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataTables.NetStandard.Enhanced.Sample.Migrations
{
    public partial class AddCreatedAtAndUpdatedAtColumnsToLocationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Locations",
                nullable: false,
                defaultValue: new DateTimeOffset(1, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Locations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Locations");
        }
    }
}
