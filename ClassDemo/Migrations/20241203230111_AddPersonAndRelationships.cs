using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment3.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "weight",
                table: "People",
                newName: "Weight");

            migrationBuilder.RenameColumn(
                name: "time",
                table: "People",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "sex",
                table: "People",
                newName: "Sex");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "People",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "goalWeight",
                table: "People",
                newName: "GoalWeight");

            migrationBuilder.RenameColumn(
                name: "age",
                table: "People",
                newName: "Age");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "People",
                newName: "weight");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "People",
                newName: "time");

            migrationBuilder.RenameColumn(
                name: "Sex",
                table: "People",
                newName: "sex");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "People",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "GoalWeight",
                table: "People",
                newName: "goalWeight");

            migrationBuilder.RenameColumn(
                name: "Age",
                table: "People",
                newName: "age");
        }
    }
}
