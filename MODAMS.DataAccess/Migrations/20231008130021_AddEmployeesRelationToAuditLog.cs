using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MODAMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeesRelationToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EmployeeId",
                table: "AuditLog",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_Employees_EmployeeId",
                table: "AuditLog",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_Employees_EmployeeId",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_EmployeeId",
                table: "AuditLog");
        }
    }
}
