using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FindFun.Server.Migrations
{
    /// <inheritdoc />
    public partial class pendomgChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_closing_schedules_parks_park_id",
                table: "closing_schedules");

            migrationBuilder.AddForeignKey(
                name: "FK_closing_schedules_parks_park_id",
                table: "closing_schedules",
                column: "park_id",
                principalTable: "parks",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_closing_schedules_parks_park_id",
                table: "closing_schedules");

            migrationBuilder.AddForeignKey(
                name: "FK_closing_schedules_parks_park_id",
                table: "closing_schedules",
                column: "park_id",
                principalTable: "parks",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
