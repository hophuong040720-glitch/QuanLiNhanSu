using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    // ==========================================
    // CÁC BẢNG QUẢN LÝ NGHIỆP VỤ (ERP)
    // ==========================================

    // 1. CHẤM CÔNG
    public class ChamCong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaNV { get; set; } = null!;

        public DateTime NgayChamCong { get; set; } // Ngày chấm công (yyyy-MM-dd)
        public DateTime? GioVao { get; set; }       // Giờ Check-in
        public DateTime? GioRa { get; set; }        // Giờ Check-out
        public double SoGioLam { get; set; }         // Số tiếng làm việc thực tế
        public string TrangThai { get; set; } = "Chờ Check-out"; // Đi làm, Nửa ngày, Chờ Check-out
        public string? GhiChu { get; set; }
    }

    // 2. ỨNG LƯƠNG
    public class UngLuong
    {
        [Key] public int Id { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        public DateTime NgayYeuCau { get; set; }
        public decimal SoTien { get; set; }
        public string LyDo { get; set; } = null!;
        public string TrangThai { get; set; } = "Chờ duyệt";
    }

    // 3. PHÂN CÔNG
    public class PhanCong
    {
        [Key] public int Id { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        public string TenCongViec { get; set; } = null!;
        public string? MoTa { get; set; }
        public DateTime HanChot { get; set; }
        public string TrangThai { get; set; } = "Mới giao";
    }

    // 4. BẢNG LƯƠNG CHỐT THÁNG
    public class BangLuong
    {
        [Key] public int Id { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal LuongCoBan { get; set; }
        public int SoNgayDiLam { get; set; }
        public decimal TienUng { get; set; }
        public decimal TienThuong { get; set; }
        public decimal TienPhat { get; set; }
        public decimal ThucLanh { get; set; }
    }

    // ==========================================
    // CÁC BẢNG CHUẨN HÓA ERD (TỪ BÁO CÁO)
    // ==========================================

    // 5. PHÒNG BAN
    public class PhongBan
    {
        [Key] public int MaPB { get; set; }
        [Required][StringLength(100)] public string TenPB { get; set; } = null!;
        public string? MaTruongPhong { get; set; }
        public string? DiaDiem { get; set; }
    }

    // 6. CHỨC VỤ
    public class ChucVu
    {
        [Key] public int MaCV { get; set; }
        [Required][StringLength(100)] public string TenCV { get; set; } = null!;
        public decimal HeSoPhuCap { get; set; }
    }

    // 7. HỢP ĐỒNG LAO ĐỘNG
    public class HopDong
    {
        [Key] public int MaHD { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        [Required] public string LoaiHD { get; set; } = null!;
        public DateTime NgayKy { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public string? NoiDung { get; set; }
    }

    // 8. KHEN THƯỞNG KỶ LUẬT
    public class KhenThuongKyLuat
    {
        [Key] public int MaKTKL { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        [Required] public string HinhThuc { get; set; } = null!;
        [Required] public string Loai { get; set; } = null!;
        public decimal SoTien { get; set; }
        public DateTime NgayQuyetDinh { get; set; }
        public string? NoiDung { get; set; }
    }

    // 9. PHIẾU NGHỈ PHÉP
    public class PhieuNghiPhep
    {
        [Key] public int MaPhieu { get; set; }
        [Required] public string MaNV { get; set; } = null!;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string? LyDo { get; set; }
        public string TrangThai { get; set; } = "Chờ duyệt";
    }
}