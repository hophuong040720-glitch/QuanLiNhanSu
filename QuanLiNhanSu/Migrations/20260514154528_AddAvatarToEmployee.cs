using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Employees");
        }
    }
}
