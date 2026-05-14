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
            if (User.Identity!.IsAuthenticated)
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
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

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
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng lặp Username
                var userExists = _context.Users.Any(u => u.Username == model.Username);
                if (userExists)
                {
                    ViewBag.Error = "Tên đăng nhập đã có người sử dụng!";
                    return View(model);
                }

                // Chốt kiểm soát phân quyền: Chỉ cho phép nhận quyền Employee hoặc Guest từ giao diện
                var safeRole = (model.Role == "Employee" || model.Role == "Guest") ? model.Role : "Guest";

                var newUser = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email,
                    Role = safeRole
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đăng ký thành công tài khoản với quyền {safeRole}!";
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
                ViewBag.Success = $"Cảm ơn {model.HoTen}. Yêu cầu hỗ trợ của bạn đã được gửi tới Admin!";
                return View();
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}