using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeService.AttendanceMigrations
{
    /// <inheritdoc />
    public partial class InitialAttendanceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScannedInByManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScannedInByManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScannedOutByManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScannedOutByManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QrNonce = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QrExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PendingQrType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsNonceConsumed = table.Column<bool>(type: "bit", nullable: false),
                    IsQrExpired = table.Column<bool>(type: "bit", nullable: false),
                    ClockInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClockOutAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeUserId_AttendanceDate",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeUserId", "AttendanceDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");
        }
    }
}
