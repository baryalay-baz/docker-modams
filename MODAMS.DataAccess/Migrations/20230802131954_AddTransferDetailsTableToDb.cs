using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferDetailsTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrevStoreId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferDetails_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_AssetId",
                table: "TransferDetails",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferDetails_TransferId",
                table: "TransferDetails",
                column: "TransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferDetails");
        }
    }
}
