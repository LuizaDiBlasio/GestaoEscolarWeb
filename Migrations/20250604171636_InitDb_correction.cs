using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class InitDb_correction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserIdStudentId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "UserIdStudentId",
                table: "Students",
                newName: "UserStudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_UserIdStudentId",
                table: "Students",
                newName: "IX_Students_UserStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserStudentId",
                table: "Students",
                column: "UserStudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserStudentId",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "UserStudentId",
                table: "Students",
                newName: "UserIdStudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_UserStudentId",
                table: "Students",
                newName: "IX_Students_UserIdStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserIdStudentId",
                table: "Students",
                column: "UserIdStudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
