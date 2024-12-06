using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment3.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nutrition_People_PersonId",
                table: "Nutrition");

            migrationBuilder.DropIndex(
                name: "IX_Nutrition_PersonId",
                table: "Nutrition");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Nutrition",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Nutrition_PersonId",
                table: "Nutrition",
                column: "PersonId",
                unique: true,
                filter: "[PersonId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Nutrition_People_PersonId",
                table: "Nutrition",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nutrition_People_PersonId",
                table: "Nutrition");

            migrationBuilder.DropIndex(
                name: "IX_Nutrition_PersonId",
                table: "Nutrition");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Nutrition",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nutrition_PersonId",
                table: "Nutrition",
                column: "PersonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Nutrition_People_PersonId",
                table: "Nutrition",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
