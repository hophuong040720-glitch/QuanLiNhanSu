using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class Employee
    {
        [Key] // Xác định đây là khóa chính
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string MaNV { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        public string ChucVu { get; set; }

        public decimal Luong { get; set; }

        public string PhongBan { get; set; }
    }
}