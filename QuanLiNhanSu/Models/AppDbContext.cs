using System;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Services;

namespace QuanLiNhanSu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 1. Các bảng cốt lõi
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkReport> WorkReports { get; set; }

        // 2. Các bảng ERP nghiệp vụ
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<UngLuong> UngLuongs { get; set; }
        public DbSet<PhanCong> PhanCongs { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }

        // 3. Các bảng chuẩn hóa ERD 3NF
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<ChucVu> ChucVus { get; set; }
        public DbSet<HopDong> HopDongs { get; set; }
        public DbSet<KhenThuongKyLuat> KhenThuongKyLuats { get; set; }
        public DbSet<PhieuNghiPhep> PhieuNghiPheps { get; set; }

        // 4. Audit
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================================
            // CẤU HÌNH PRECISION CHO CÁC CỘT TIỀN TỆ
            // =============================================
            modelBuilder.Entity<Employee>().Property(e => e.Luong).HasPrecision(18, 0);
            modelBuilder.Entity<UngLuong>().Property(u => u.SoTien).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.LuongCoBan).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.TienUng).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.TienThuong).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.TienPhat).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.ThucLanh).HasPrecision(18, 0);
            modelBuilder.Entity<ChucVu>().Property(c => c.HeSoPhuCap).HasPrecision(18, 2);
            modelBuilder.Entity<KhenThuongKyLuat>().Property(k => k.SoTien).HasPrecision(18, 0);

            // =============================================
            // QUAN HỆ KHÓA NGOẠI: Employee ↔ PhongBan
            // =============================================
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.PhongBanNav)
                .WithMany()
                .HasForeignKey(e => e.PhongBanId)
                .OnDelete(DeleteBehavior.SetNull);  // Khi xóa PhongBan, NV chưa được phân công (=NULL)

            // =============================================
            // QUAN HỆ KHÓA NGOẠI: Employee ↔ ChucVu
            // =============================================
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ChucVuNav)
                .WithMany()
                .HasForeignKey(e => e.ChucVuId)
                .OnDelete(DeleteBehavior.SetNull);  // Khi xóa ChucVu, NV chưa được phân chức vụ (=NULL)

            // =============================================
            // SEED DATA - DỮ LIỆU MẶC ĐỊNH
            // =============================================

            // Seed Phòng Ban
            modelBuilder.Entity<PhongBan>().HasData(
                new PhongBan { MaPB = 1, TenPB = "Ban Giám Đốc",   DiaDiem = "Tầng 5" },
                new PhongBan { MaPB = 2, TenPB = "IT",              DiaDiem = "Tầng 3" },
                new PhongBan { MaPB = 3, TenPB = "Kế toán",         DiaDiem = "Tầng 2" },
                new PhongBan { MaPB = 4, TenPB = "Nhân sự",         DiaDiem = "Tầng 2" },
                new PhongBan { MaPB = 5, TenPB = "Kinh doanh",      DiaDiem = "Tầng 4" },
                new PhongBan { MaPB = 6, TenPB = "Marketing",       DiaDiem = "Tầng 4" },
                new PhongBan { MaPB = 7, TenPB = "Hành chính",      DiaDiem = "Tầng 1" }
            );

            // Seed Chức Vụ (kèm hệ số phụ cấp)
            modelBuilder.Entity<ChucVu>().HasData(
                new ChucVu { MaCV = 1, TenCV = "Giám đốc",        HeSoPhuCap = 3.0m },
                new ChucVu { MaCV = 2, TenCV = "Phó Giám đốc",    HeSoPhuCap = 2.5m },
                new ChucVu { MaCV = 3, TenCV = "Trưởng phòng",    HeSoPhuCap = 2.0m },
                new ChucVu { MaCV = 4, TenCV = "Phó phòng",       HeSoPhuCap = 1.7m },
                new ChucVu { MaCV = 5, TenCV = "Chuyên viên",     HeSoPhuCap = 1.3m },
                new ChucVu { MaCV = 6, TenCV = "Developer",       HeSoPhuCap = 1.5m },
                new ChucVu { MaCV = 7, TenCV = "Sale Manager",    HeSoPhuCap = 1.6m },
                new ChucVu { MaCV = 8, TenCV = "Nhân viên",       HeSoPhuCap = 1.0m },
                new ChucVu { MaCV = 9, TenCV = "Thực tập sinh",   HeSoPhuCap = 0.7m }
            );
        }
    }
}