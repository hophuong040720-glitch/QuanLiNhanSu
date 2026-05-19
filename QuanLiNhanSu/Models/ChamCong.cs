using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }
        public string MaNV { get; set; } = null!;
        public DateTime NgayChamCong { get; set; }
        public string TrangThai { get; set; } = "Đi làm"; 
        public string? GhiChu { get; set; }
    }
}