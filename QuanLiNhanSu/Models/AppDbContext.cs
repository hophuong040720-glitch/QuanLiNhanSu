using Microsoft.EntityFrameworkCore;

namespace QuanLiNhanSu.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Bảng tài khoản (đã có)
        public DbSet<User> Users { get; set; }

        // Thêm dòng này để tạo bảng Nhân viên
        public DbSet<Employee> Employees { get; set; }
    }
}