using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class BangLuong
    {
        [Key]
        public int Id { get; set; }
        public string MaNV { get; set; } = null!;
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal LuongCoBan { get; set; } // Lấy từ Employee
        public int SoNgayDiLam { get; set; } // Đếm từ bảng ChamCong
        public decimal TienUng { get; set; } // Tổng từ bảng UngLuong
        public decimal ThucLanh { get; set; } // (LuongCoBan / 26 * SoNgayDiLam) - TienUng
    }
}