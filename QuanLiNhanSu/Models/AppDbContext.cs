using Microsoft.EntityFrameworkCore;

namespace QuanLiNhanSu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Bảng tài khoản (đã có)
        public DbSet<User> Users { get; set; }

        // Bảng Nhân viên và Báo cáo công việc (đã có)
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkReport> WorkReports { get; set; }

        // CẬP NHẬT MỚI: ĐĂNG KÝ 4 BẢNG MINI-ERP VÀO DATABASE
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<UngLuong> UngLuongs { get; set; }
        public DbSet<PhanCong> PhanCongs { get; set; }
        public DbSet<BangLuong> BangLuongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình độ chính xác của trường Lương trong bảng Employee
            modelBuilder.Entity<Employee>()
                .Property(e => e.Luong)
                .HasPrecision(18, 0);

            // CẬP NHẬT MỚI: Cấu hình độ chính xác cho trường Số tiền ứng và Thựclãnh
            modelBuilder.Entity<UngLuong>()
                .Property(u => u.SoTien)
                .HasPrecision(18, 0);

            modelBuilder.Entity<BangLuong>()
                .Property(b => b.LuongCoBan)
                .HasPrecision(18, 0);

            modelBuilder.Entity<BangLuong>()
                .Property(b => b.TienUng)
                .HasPrecision(18, 0);

            modelBuilder.Entity<BangLuong>()
                .Property(b => b.ThucLanh)
                .HasPrecision(18, 0);
        }
    }
}