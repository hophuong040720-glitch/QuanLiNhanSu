using System.ComponentModel.DataAnnotations;

namespace QuanLiNhanSu.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email đã đăng ký")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        public string NewPassword { get; set; }
    }

    public class ContactAdminViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên của bạn")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung cần hỗ trợ")]
        public string NoiDung { get; set; }
    }
}