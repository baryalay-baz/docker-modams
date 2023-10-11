using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginHistory_Employees_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoginHistory_EmployeeId",
                table: "LoginHistory",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginHistory_Employees_EmployeeId",
                table: "LoginHistory",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginHistory_Employees_EmployeeId",
                table: "LoginHistory");

            migrationBuilder.DropIndex(
                name: "IX_LoginHistory_EmployeeId",
                table: "LoginHistory");
        }
    }
}
