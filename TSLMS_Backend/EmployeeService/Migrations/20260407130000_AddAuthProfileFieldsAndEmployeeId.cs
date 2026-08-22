using EmployeeService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260407130000_AddAuthProfileFieldsAndEmployeeId")]
    public partial class AddAuthProfileFieldsAndEmployeeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
