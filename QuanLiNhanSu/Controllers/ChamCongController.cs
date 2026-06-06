using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ChamCongController : Controller
    {
        private readonly AppDbContext _context;

        public ChamCongController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ChamCong
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var logs = _context.ChamCongs.AsQueryable();

            // Nếu không phải Sếp hoặc Kế toán, chỉ cho xem lịch sử của chính mình
            if (!User.IsInRole("Admin") && !User.IsInRole("KeToan"))
            {
                logs = logs.Where(c => c.MaNV == currentUser);
            }

            // Lấy Avatar của nhân viên đang đăng nhập để dùng cho Face ID
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.MaNV == currentUser);
            ViewBag.UserAvatar = employee?.AvatarUrl ?? "/images/default-avatar.png";

            return View(await logs.OrderByDescending(c => c.NgayChamCong).ToListAsync());
        }

        // POST: ChamCong/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string trangThai, string? ghiChu)
        {
            var currentUser = User.Identity!.Name;
            var today = DateTime.Today;

            // Chốt chặn bảo mật: Mỗi ngày chỉ được bấm chấm công 1 lần độc nhất
            var alreadyCheckIn = await _context.ChamCongs
                .AnyAsync(c => c.MaNV == currentUser && c.NgayChamCong.Date == today);

            if (alreadyCheckIn)
            {
                TempData["Error"] = "Hệ thống từ chối! Hôm nay bạn đã thực hiện điểm danh chấm công rồi.";
                return RedirectToAction(nameof(Index));
            }

            var record = new ChamCong
            {
                MaNV = currentUser ?? "Ẩn danh",
                NgayChamCong = DateTime.Now,
                TrangThai = string.IsNullOrEmpty(trangThai) ? "Đi làm" : trangThai,
                GhiChu = ghiChu
            };

            _context.ChamCongs.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ghi nhận dữ liệu chấm công ngày hôm nay thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}