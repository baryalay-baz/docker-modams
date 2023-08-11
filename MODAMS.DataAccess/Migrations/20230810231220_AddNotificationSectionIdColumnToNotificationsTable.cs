using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSectionIdColumnToNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationSectionId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationSectionId",
                table: "Notifications",
                column: "NotificationSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationSections_NotificationSectionId",
                table: "Notifications",
                column: "NotificationSectionId",
                principalTable: "NotificationSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationSections_NotificationSectionId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_NotificationSectionId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "NotificationSectionId",
                table: "Notifications");
        }
    }
}
