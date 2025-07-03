using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPropretyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyKey",
                columns: table => new
                {
                    BatchJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyKey", x => new { x.BatchJobId, x.Id });
                    table.ForeignKey(
                        name: "FK_PropertyKey_BatchJob_BatchJobId",
                        column: x => x.BatchJobId,
                        principalTable: "BatchJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyKeyValuePair",
                columns: table => new
                {
                    BatchJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyKeyValuePair", x => new { x.BatchJobId, x.Id });
                    table.ForeignKey(
                        name: "FK_PropertyKeyValuePair_BatchJob_BatchJobId",
                        column: x => x.BatchJobId,
                        principalTable: "BatchJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyKey");

            migrationBuilder.DropTable(
                name: "PropertyKeyValuePair");
        }
    }
}
