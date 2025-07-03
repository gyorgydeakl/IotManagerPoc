using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameAndAddTagEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceId_DeviceGroups_DeviceGroupId",
                table: "DeviceId");

            migrationBuilder.RenameColumn(
                name: "DeviceGroupId",
                table: "DeviceId",
                newName: "BatchJobId");

            migrationBuilder.CreateTable(
                name: "TagKey",
                columns: table => new
                {
                    BatchJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagKey", x => new { x.BatchJobId, x.Id });
                    table.ForeignKey(
                        name: "FK_TagKey_DeviceGroups_BatchJobId",
                        column: x => x.BatchJobId,
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagKeyValuePair",
                columns: table => new
                {
                    BatchJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagKeyValuePair", x => new { x.BatchJobId, x.Id });
                    table.ForeignKey(
                        name: "FK_TagKeyValuePair_DeviceGroups_BatchJobId",
                        column: x => x.BatchJobId,
                        principalTable: "DeviceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceId_DeviceGroups_BatchJobId",
                table: "DeviceId",
                column: "BatchJobId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceId_DeviceGroups_BatchJobId",
                table: "DeviceId");

            migrationBuilder.DropTable(
                name: "TagKey");

            migrationBuilder.DropTable(
                name: "TagKeyValuePair");

            migrationBuilder.RenameColumn(
                name: "BatchJobId",
                table: "DeviceId",
                newName: "DeviceGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceId_DeviceGroups_DeviceGroupId",
                table: "DeviceId",
                column: "DeviceGroupId",
                principalTable: "DeviceGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
