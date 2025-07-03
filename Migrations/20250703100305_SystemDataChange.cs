using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class SystemDataChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PassingGrade",
                table: "SystemData",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingGrade",
                table: "SystemData");
        }
    }
}
