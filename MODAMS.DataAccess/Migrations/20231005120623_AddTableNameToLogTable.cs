using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNameToLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Action",
                table: "Log",
                newName: "FieldName");

            migrationBuilder.AddColumn<int>(
                name: "ChangeType",
                table: "Log",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecordId",
                table: "Log",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "Log");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Log");

            migrationBuilder.RenameColumn(
                name: "FieldName",
                table: "Log",
                newName: "Action");
        }
    }
}
