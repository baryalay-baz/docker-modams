using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisposalImagesTableAddImageUrlColumnToDisposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisposalImages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DisposalDate",
                table: "Disposals",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Disposals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Disposals");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DisposalDate",
                table: "Disposals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.CreateTable(
                name: "DisposalImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisposalId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisposalImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisposalImages_Disposals_DisposalId",
                        column: x => x.DisposalId,
                        principalTable: "Disposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisposalImages_DisposalId",
                table: "DisposalImages",
                column: "DisposalId");
        }
    }
}
