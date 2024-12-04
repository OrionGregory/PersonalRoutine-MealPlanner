using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment3.Migrations
{
    /// <inheritdoc />
    public partial class FixPersonRoutine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routines_PersonId",
                table: "Routines");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_PersonId",
                table: "Routines",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Routines_PersonId",
                table: "Routines");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_PersonId",
                table: "Routines",
                column: "PersonId",
                unique: true);
        }
    }
}
