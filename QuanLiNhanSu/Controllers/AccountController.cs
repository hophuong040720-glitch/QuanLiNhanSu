using Microsoft.AspNetCore.Mvc;
using QuanLiNhanSu.Models;
using System.Linq;

namespace QuanLiNhanSu.Controllers
{
    // Bỏ chữ partial đi cho đơn giản nếu không cần thiết
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Trang hiển thị giao diện đăng nhập (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Hàm xử lý khi nhấn nút Đăng nhập (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kiểm tra database
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Đăng nhập thành công
                return RedirectToAction("Index", "Home");
            }

            // Nếu sai, báo lỗi
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }
    }
}