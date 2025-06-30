using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameTOBatchJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceId_DeviceGroups_BatchJobId",
                table: "DeviceId");

            migrationBuilder.DropForeignKey(
                name: "FK_TagKey_DeviceGroups_BatchJobId",
                table: "TagKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TagKeyValuePair_DeviceGroups_BatchJobId",
                table: "TagKeyValuePair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceGroups",
                table: "DeviceGroups");

            migrationBuilder.RenameTable(
                name: "DeviceGroups",
                newName: "BatchJob");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceGroups_Name",
                table: "BatchJob",
                newName: "IX_BatchJob_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchJob",
                table: "BatchJob",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceId_BatchJob_BatchJobId",
                table: "DeviceId",
                column: "BatchJobId",
                principalTable: "BatchJob",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagKey_BatchJob_BatchJobId",
                table: "TagKey",
                column: "BatchJobId",
                principalTable: "BatchJob",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagKeyValuePair_BatchJob_BatchJobId",
                table: "TagKeyValuePair",
                column: "BatchJobId",
                principalTable: "BatchJob",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceId_BatchJob_BatchJobId",
                table: "DeviceId");

            migrationBuilder.DropForeignKey(
                name: "FK_TagKey_BatchJob_BatchJobId",
                table: "TagKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TagKeyValuePair_BatchJob_BatchJobId",
                table: "TagKeyValuePair");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchJob",
                table: "BatchJob");

            migrationBuilder.RenameTable(
                name: "BatchJob",
                newName: "DeviceGroups");

            migrationBuilder.RenameIndex(
                name: "IX_BatchJob_Name",
                table: "DeviceGroups",
                newName: "IX_DeviceGroups_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceGroups",
                table: "DeviceGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceId_DeviceGroups_BatchJobId",
                table: "DeviceId",
                column: "BatchJobId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagKey_DeviceGroups_BatchJobId",
                table: "TagKey",
                column: "BatchJobId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagKeyValuePair_DeviceGroups_BatchJobId",
                table: "TagKeyValuePair",
                column: "BatchJobId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
