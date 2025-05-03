using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSomaliColumnInDictionaryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<string>(
                name: "StatusSo",
                table: "TransferStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategoryNameSo",
                table: "SubCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameSo",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TypeSo",
                table: "DisposalTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameSo",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConditionNameSo",
                table: "Conditions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryNameSo",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusNameSo",
                table: "AssetStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "StatusSo",
                table: "TransferStatuses");

            migrationBuilder.DropColumn(
                name: "SubCategoryNameSo",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "NameSo",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "TypeSo",
                table: "DisposalTypes");

            migrationBuilder.DropColumn(
                name: "NameSo",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ConditionNameSo",
                table: "Conditions");

            migrationBuilder.DropColumn(
                name: "CategoryNameSo",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StatusNameSo",
                table: "AssetStatuses");
        }
    }
}
