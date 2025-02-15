﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBackgroundColumnToNotificationSectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "background",
                table: "NotificationSections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "background",
                table: "NotificationSections");
        }
    }
}
