using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class Enum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Enrollments SET StudentStatus = 0 WHERE StudentStatus IS NULL;");


            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserAuditId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserAuditId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserAuditId",
                table: "Students");

            migrationBuilder.AlterColumn<int>(
                name: "StudentStatus",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAuditId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentStatus",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserAuditId",
                table: "Students",
                column: "UserAuditId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserAuditId",
                table: "Students",
                column: "UserAuditId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
