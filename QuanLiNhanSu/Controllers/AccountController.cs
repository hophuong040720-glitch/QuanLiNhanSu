using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuanLiNhanSu.Models;
using QuanLiNhanSu.Services;

namespace QuanLiNhanSu.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _pwdService;
        private readonly AuditService _audit;

        public AccountController(AppDbContext context, PasswordService pwdService, AuditService audit)
        {
            _context = context;
            _pwdService = pwdService;
            _audit = audit;
        }

        // ================= 1. ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null && _pwdService.VerifyPassword(password, user.Password))
            {
                // AUTO-MIGRATE: Nếu mật khẩu cũ là plain text, hash lại ngay sau khi login thành công
                if (!_pwdService.IsHashed(user.Password))
                {
                    user.Password = _pwdService.HashPassword(password);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // Ghi audit log đăng nhập thành công
                await _audit.LogAsync(user.Username, "Đăng nhập", "Users",
                    $"Tài khoản [{user.Username}] đăng nhập thành công. Quyền: {user.Role}.");

                return RedirectToAction("Index", "Home");
            }

            // Ghi log đăng nhập thất bại
            await _audit.LogAsync(username ?? "Unknown", "Đăng nhập thất bại", "Users",
                $"Ai đó nhập sai mật khẩu cho tài khoản [{username}].");

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        // ================= 2. ĐĂNG XUẤT =================
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";
            await _audit.LogAsync(username, "Đăng xuất", "Users",
                $"Tài khoản [{username}] đã đăng xuất khỏi hệ thống.");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ================= 3. ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userExists = _context.Users.Any(u => u.Username == model.Username);
                if (userExists)
                {
                    ViewBag.Error = "Tên đăng nhập đã có người sử dụng!";
                    return View(model);
                }

                // Chốt phân quyền: giao diện đăng ký chỉ cho phép Employee
                var safeRole = (model.Role == "Employee") ? "Employee" : "Employee";

                var newUser = new User
                {
                    Username = model.Username,
                    Password = _pwdService.HashPassword(model.Password), // Hash ngay khi tạo
                    Email = model.Email,
                    Role = safeRole
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(model.Username, "Đăng ký tài khoản", "Users",
                    $"Tài khoản mới [{model.Username}] được tạo. Quyền: {safeRole}.");

                TempData["Success"] = $"Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ================= 4. QUÊN MẬT KHẨU =================
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Email == model.Email);
                if (user != null)
                {
                    user.Password = _pwdService.HashPassword(model.NewPassword); // Hash mật khẩu mới
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    await _audit.LogAsync(model.Username, "Đổi mật khẩu", "Users",
                        $"Tài khoản [{model.Username}] đặt lại mật khẩu qua trang Quên mật khẩu.");

                    ViewBag.Success = "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới.";
                    return View();
                }
                ViewBag.Error = "Tên đăng nhập hoặc Email không khớp với hệ thống!";
            }
            return View(model);
        }

        // ================= 5. LIÊN HỆ ADMIN =================
        [HttpGet]
        public IActionResult ContactAdmin() => View();

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
        public IActionResult AccessDenied() => View();
    }
}