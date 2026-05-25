using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLiNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GioRa",
                table: "ChamCongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GioVao",
                table: "ChamCongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SoGioLam",
                table: "ChamCongs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "GioRa",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "GioVao",
                table: "ChamCongs");

            migrationBuilder.DropColumn(
                name: "SoGioLam",
                table: "ChamCongs");
        }
    }
}
