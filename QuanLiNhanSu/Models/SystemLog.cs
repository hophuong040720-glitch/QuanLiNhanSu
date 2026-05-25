using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class SystemLog
    {
        [Key] public int Id { get; set; }
        public string Username { get; set; } = null!; // Tên tài khoản thực hiện
        public string Action { get; set; } = null!;   // Hành động (Thêm/Sửa/Xóa/Chốt Lương)
        public string Target { get; set; } = null!;   // Bảng bị tác động (Ví dụ: Bảng Lương)
        public DateTime Timestamp { get; set; } = DateTime.Now; // Thời gian thực hiện
        public string? Details { get; set; }          // Chi tiết (Ví dụ: Sửa lương từ 5tr -> 10tr)
    }
}