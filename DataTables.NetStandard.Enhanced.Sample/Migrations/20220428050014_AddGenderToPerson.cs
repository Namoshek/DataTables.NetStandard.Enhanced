using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataTables.NetStandard.Enhanced.Sample.Migrations
{
    public partial class AddGenderToPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Persons");
        }
    }
}
