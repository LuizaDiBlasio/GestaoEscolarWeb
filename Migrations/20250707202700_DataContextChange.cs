using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class DataContextChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CreditHours_Name",
                table: "Subjects",
                columns: new[] { "CreditHours", "Name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_CreditHours_Name",
                table: "Subjects");
        }
    }
}
