using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddMiniERPModulesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BangLuongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    LuongCoBan = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    SoNgayDiLam = table.Column<int>(type: "int", nullable: false),
                    TienUng = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    ThucLanh = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangLuongs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChamCongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayChamCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChamCongs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenCongViec = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HanChot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UngLuongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UngLuongs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangLuongs");

            migrationBuilder.DropTable(
                name: "ChamCongs");

            migrationBuilder.DropTable(
                name: "PhanCongs");

            migrationBuilder.DropTable(
                name: "UngLuongs");
        }
    }
}
