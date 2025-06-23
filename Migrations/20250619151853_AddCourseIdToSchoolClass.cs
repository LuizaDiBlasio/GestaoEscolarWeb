using Microsoft.EntityFrameworkCore.Migrations;

namespace GestaoEscolarWeb.Migrations
{
    public partial class AddCourseIdToSchoolClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId",
                table: "SchoolClasses");

            migrationBuilder.DropIndex(
                name: "IX_SchoolClasses_CourseId",
                table: "SchoolClasses");

            migrationBuilder.DropColumn(
                name: "Course",
                table: "SchoolClasses");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "SchoolClasses");

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "SchoolClasses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolClasses_CourseId1",
                table: "SchoolClasses",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId1",
                table: "SchoolClasses",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId1",
                table: "SchoolClasses");

            migrationBuilder.DropIndex(
                name: "IX_SchoolClasses_CourseId1",
                table: "SchoolClasses");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "SchoolClasses");

            migrationBuilder.AddColumn<string>(
                name: "Course",
                table: "SchoolClasses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "SchoolClasses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolClasses_CourseId",
                table: "SchoolClasses",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolClasses_Courses_CourseId",
                table: "SchoolClasses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
