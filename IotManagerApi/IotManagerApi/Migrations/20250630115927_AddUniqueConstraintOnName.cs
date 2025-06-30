using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintOnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeviceGroups_Name",
                table: "DeviceGroups",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceGroups_Name",
                table: "DeviceGroups");
        }
    }
}
