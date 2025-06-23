using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class SchoolClass2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId1",
                table: "SchoolClasses");

            migrationBuilder.RenameColumn(
                name: "CourseId1",
                table: "SchoolClasses",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_SchoolClasses_CourseId1",
                table: "SchoolClasses",
                newName: "IX_SchoolClasses_CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId",
                table: "SchoolClasses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId",
                table: "SchoolClasses");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "SchoolClasses",
                newName: "CourseId1");

            migrationBuilder.RenameIndex(
                name: "IX_SchoolClasses_CourseId",
                table: "SchoolClasses",
                newName: "IX_SchoolClasses_CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId1",
                table: "SchoolClasses",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
