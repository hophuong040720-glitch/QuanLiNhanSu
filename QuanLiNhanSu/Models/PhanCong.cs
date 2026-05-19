using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class PhanCong
    {
        [Key]
        public int Id { get; set; }
        public string MaNV { get; set; } = null!; // Người được giao
        public string TenCongViec { get; set; } = null!;
        public string? MoTa { get; set; }
        public DateTime HanChot { get; set; }
        public string TrangThai { get; set; } = "Mới giao"; // Mới giao, Đang làm, Hoàn thành, Trễ hạn
    }
}