using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLiNhanSu.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_NormalizeDB_BCrypt_AuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChucVu",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhongBan",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "MaNV",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ChucVuId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhongBanId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "ChucVus",
                columns: new[] { "MaCV", "HeSoPhuCap", "TenCV" },
                values: new object[,]
                {
                    { 1, 3.0m, "Giám đốc" },
                    { 2, 2.5m, "Phó Giám đốc" },
                    { 3, 2.0m, "Trưởng phòng" },
                    { 4, 1.7m, "Phó phòng" },
                    { 5, 1.3m, "Chuyên viên" },
                    { 6, 1.5m, "Developer" },
                    { 7, 1.6m, "Sale Manager" },
                    { 8, 1.0m, "Nhân viên" },
                    { 9, 0.7m, "Thực tập sinh" }
                });

            migrationBuilder.InsertData(
                table: "PhongBans",
                columns: new[] { "MaPB", "DiaDiem", "MaTruongPhong", "TenPB" },
                values: new object[,]
                {
                    { 1, "Tầng 5", null, "Ban Giám Đốc" },
                    { 2, "Tầng 3", null, "IT" },
                    { 3, "Tầng 2", null, "Kế toán" },
                    { 4, "Tầng 2", null, "Nhân sự" },
                    { 5, "Tầng 4", null, "Kinh doanh" },
                    { 6, "Tầng 4", null, "Marketing" },
                    { 7, "Tầng 1", null, "Hành chính" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ChucVuId",
                table: "Employees",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PhongBanId",
                table: "Employees",
                column: "PhongBanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ChucVus_ChucVuId",
                table: "Employees",
                column: "ChucVuId",
                principalTable: "ChucVus",
                principalColumn: "MaCV",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_PhongBans_PhongBanId",
                table: "Employees",
                column: "PhongBanId",
                principalTable: "PhongBans",
                principalColumn: "MaPB",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ChucVus_ChucVuId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_PhongBans_PhongBanId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ChucVuId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PhongBanId",
                table: "Employees");

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ChucVus",
                keyColumn: "MaCV",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PhongBans",
                keyColumn: "MaPB",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "ChucVuId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhongBanId",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "MaNV",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "ChucVu",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhongBan",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
