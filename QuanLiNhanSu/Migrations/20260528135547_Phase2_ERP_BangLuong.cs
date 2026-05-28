using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_ERP_BangLuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TienPhat",
                table: "BangLuongs",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TienThuong",
                table: "BangLuongs",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TienPhat",
                table: "BangLuongs");

            migrationBuilder.DropColumn(
                name: "TienThuong",
                table: "BangLuongs");
        }
    }
}
