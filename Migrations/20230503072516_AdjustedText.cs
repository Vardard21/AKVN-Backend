using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AKVN_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Texts_Actors_OwnerId",
                table: "Texts");

            migrationBuilder.DropIndex(
                name: "IX_Texts_OwnerId",
                table: "Texts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Texts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Texts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Texts_OwnerId",
                table: "Texts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Texts_Actors_OwnerId",
                table: "Texts",
                column: "OwnerId",
                principalTable: "Actors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
