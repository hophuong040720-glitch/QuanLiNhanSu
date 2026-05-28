using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLiNhanSu.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
        [StringLength(20, ErrorMessage = "Mã nhân viên tối đa 20 ký tự")]
        [Display(Name = "Mã nhân viên")]
        public string MaNV { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; } = null!;

        // ==========================================
        // CHUẨN HÓA FK: PhongBan → bảng PhongBans
        // ==========================================
        [Display(Name = "Phòng Ban")]
        public int? PhongBanId { get; set; }

        [ForeignKey("PhongBanId")]
        public PhongBan? PhongBanNav { get; set; }

        // ==========================================
        // CHUẨN HÓA FK: ChucVu → bảng ChucVus
        // ==========================================
        [Display(Name = "Chức Vụ")]
        public int? ChucVuId { get; set; }

        [ForeignKey("ChucVuId")]
        public ChucVu? ChucVuNav { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mức lương")]
        [Display(Name = "Lương Cơ Bản (VNĐ)")]
        [Range(0, double.MaxValue, ErrorMessage = "Mức lương phải là số dương")]
        public decimal Luong { get; set; }

        // Đường dẫn ảnh Avatar
        public string? AvatarUrl { get; set; }

        // ==========================================
        // COMPUTED PROPERTIES (không lưu DB, chỉ dùng trong View)
        // ==========================================
        [NotMapped]
        public string TenPhongBan => PhongBanNav?.TenPB ?? "Chưa phân phòng";

        [NotMapped]
        public string TenChucVu => ChucVuNav?.TenCV ?? "Chưa phân chức vụ";
    }
}