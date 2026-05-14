using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
        public string MaNV { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        public string PhongBan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chức vụ")]
        public string ChucVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mức lương")]
        public decimal Luong { get; set; }

        // THÊM MỚI: Trường lưu đường dẫn ảnh Avatar
        public string? AvatarUrl { get; set; }
    }
}