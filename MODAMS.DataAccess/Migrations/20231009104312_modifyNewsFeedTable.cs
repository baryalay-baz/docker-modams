using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class modifyNewsFeedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedType",
                table: "NewsFeed",
                newName: "Controller");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "NewsFeed",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "NewsFeed",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "NewsFeed");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "NewsFeed");

            migrationBuilder.RenameColumn(
                name: "Controller",
                table: "NewsFeed",
                newName: "FeedType");
        }
    }
}
