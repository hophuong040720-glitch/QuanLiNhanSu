using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuanLiNhanSu.Models;

namespace QuanLiNhanSu.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì không cho vào trang Login nữa, đẩy thẳng ra Home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // 1. Tạo các thông tin định danh (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role) // Lưu luôn quyền Admin/User
                };

                // 2. Tạo "Thẻ bài" (Identity)
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Đăng nhập và lưu Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        // ================= 2. ĐĂNG XUẤT =================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ================= 3. ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem user gõ tên đăng nhập này đã có ai xài chưa
                var userExists = _context.Users.Any(u => u.Username == model.Username);
                if (userExists)
                {
                    ViewBag.Error = "Tên đăng nhập đã có người sử dụng!";
                    return View(model);
                }

                var newUser = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email,
                    Role = "Employee" // Mặc định tài khoản mới tạo là nhân viên
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ================= 4. QUÊN MẬT KHẨU =================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Khớp đúng Username và Email trong DB thì cho đổi mật khẩu thẳng
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Email == model.Email);
                if (user != null)
                {
                    user.Password = model.NewPassword;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    ViewBag.Success = "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới.";
                    return View();
                }
                ViewBag.Error = "Tên đăng nhập hoặc Email không khớp với hệ thống!";
            }
            return View(model);
        }

        // ================= 5. LIÊN HỆ ADMIN =================
        [HttpGet]
        public IActionResult ContactAdmin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactAdmin(ContactAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Success = "Cảm ơn " + model.HoTen + ". Yêu cầu của bạn đã được gửi tới Admin!";
                return View();
            }
            return View(model);
        }
    }
}