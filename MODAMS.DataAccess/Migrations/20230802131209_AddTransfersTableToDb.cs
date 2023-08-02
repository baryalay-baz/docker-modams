using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTransfersTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SubmissionForAcknowledgementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferStatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_TransferStatuses_TransferStatusId",
                        column: x => x.TransferStatusId,
                        principalTable: "TransferStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_StoreId",
                table: "Transfers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferStatusId",
                table: "Transfers",
                column: "TransferStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers");
        }
    }
}
