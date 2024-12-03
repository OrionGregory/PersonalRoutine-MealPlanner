using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment3.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "People",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_People_UserId",
                table: "People",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_AspNetUsers_UserId",
                table: "People",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_AspNetUsers_UserId",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_People_UserId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "People");
        }
    }
}
