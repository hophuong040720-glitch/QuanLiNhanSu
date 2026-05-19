using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class UngLuong
    {
        [Key]
        public int Id { get; set; }
        public string MaNV { get; set; } = null!;
        public DateTime NgayYeuCau { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; } = null!;
        public string TrangThai { get; set; } = "Chờ duyệt"; 
    }
}