using System;
using Microsoft.EntityFrameworkCore;

namespace QuanLiNhanSu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 1. Các bảng có sẵn (Đã nằm trong các file .cs riêng)
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkReport> WorkReports { get; set; }

        // 2. Các bảng ERP (Đang nằm trong ERD_AdditionalModels.cs hoặc file riêng)
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

        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().Property(e => e.Luong).HasPrecision(18, 0);
            modelBuilder.Entity<UngLuong>().Property(u => u.SoTien).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.LuongCoBan).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.TienUng).HasPrecision(18, 0);
            modelBuilder.Entity<BangLuong>().Property(b => b.ThucLanh).HasPrecision(18, 0);
            modelBuilder.Entity<ChucVu>().Property(c => c.HeSoPhuCap).HasPrecision(18, 2);
            modelBuilder.Entity<KhenThuongKyLuat>().Property(k => k.SoTien).HasPrecision(18, 0);
        }
    }
}