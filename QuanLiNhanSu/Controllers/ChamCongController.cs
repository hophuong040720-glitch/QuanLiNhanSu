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
        // SEED DATA HỖ TRỢ TEST
        [AllowAnonymous]
        public async Task<IActionResult> SeedData()
        {
            // Xoá toàn bộ data cũ
            _context.ChamCongs.RemoveRange(_context.ChamCongs);
            await _context.SaveChangesAsync();

            var employees = await _context.Employees.ToListAsync();
            var random = new Random();
            var chamCongs = new List<ChamCong>();

            // Tạo data cho 30 ngày qua (đến hôm nay)
            for (int i = 30; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                // Mặc định công ty nghỉ Chủ Nhật
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                foreach (var emp in employees)
                {
                    // Tỉ lệ: 85% đi làm đủ, 10% đi làm nửa ngày, 5% nghỉ phép/không chấm công
                    int roll = random.Next(100);
                    if (roll < 5) continue; 

                    var chamCong = new ChamCong
                    {
                        MaNV = emp.MaNV,
                        NgayChamCong = date,
                    };

                    if (roll < 15) // Nửa ngày (làm sáng)
                    {
                        chamCong.GioVao = date.AddHours(8).AddMinutes(random.Next(-10, 15)); // 7:50 - 8:15
                        chamCong.GioRa = date.AddHours(12).AddMinutes(random.Next(0, 15));  // 12:00 - 12:15
                        chamCong.SoGioLam = Math.Round((chamCong.GioRa.Value - chamCong.GioVao.Value).TotalHours, 1);
                        chamCong.TrangThai = "Nửa ngày";
                        chamCong.GhiChu = "Chấm công bằng Vân tay (Web)";
                    }
                    else // Đi làm cả ngày
                    {
                        chamCong.GioVao = date.AddHours(8).AddMinutes(random.Next(-15, 5));  // 7:45 - 8:05
                        chamCong.GioRa = date.AddHours(17).AddMinutes(random.Next(0, 45));   // 17:00 - 17:45
                        chamCong.SoGioLam = Math.Round((chamCong.GioRa.Value - chamCong.GioVao.Value).TotalHours, 1);
                        chamCong.TrangThai = "Đi làm";
                        chamCong.GhiChu = roll % 2 == 0 ? "Chấm công bằng Face ID" : "Chấm công bằng Vân tay (Web)";
                    }

                    chamCongs.Add(chamCong);
                }
            }

            _context.ChamCongs.AddRange(chamCongs);
            await _context.SaveChangesAsync();

            return Content($"Đã xoá dữ liệu cũ và tạo thành công {chamCongs.Count} bản ghi chấm công mới cho toàn bộ nhân viên!");
        }

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
        public async Task<IActionResult> CheckIn(string? ghiChu, string? mockShiftType)
        {
            var currentUser = User.Identity!.Name;
            var today = DateTime.Today;

            // Kiểm tra đã có bản ghi Check-in hôm nay chưa
            var existingRecord = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNV == currentUser && c.NgayChamCong.Date == today);

            if (existingRecord != null && existingRecord.GioRa != null)
            {
                TempData["Error"] = "Hệ thống từ chối! Hôm nay bạn đã Check-in và Check-out đầy đủ rồi.";
                return RedirectToAction(nameof(Index));
            }

            if (existingRecord != null && existingRecord.GioRa == null)
            {
                // KIỂM TRA CHỐNG DOUBLE-CLICK (Chỉ áp dụng nếu không dùng test mode)
                if (string.IsNullOrEmpty(mockShiftType) && existingRecord.GioVao.HasValue && (DateTime.Now - existingRecord.GioVao.Value).TotalMinutes < 1)
                {
                    TempData["Error"] = "Thao tác quá nhanh! Vui lòng thử lại sau 1 phút.";
                    return RedirectToAction(nameof(Index));
                }

                // CHECK-OUT: Giả lập giờ ra theo Test Mode
                var now = DateTime.Now;
                if (mockShiftType == "full" && existingRecord.GioVao.HasValue) 
                {
                    now = existingRecord.GioVao.Value.Date.AddHours(17).AddMinutes(new Random().Next(0, 30));
                }
                else if (mockShiftType == "half" && existingRecord.GioVao.HasValue) 
                {
                    now = existingRecord.GioVao.Value.Date.AddHours(12).AddMinutes(new Random().Next(0, 15));
                }

                existingRecord.GioRa = now;
                double duration = 0;
                if (existingRecord.GioVao.HasValue)
                {
                    duration = (existingRecord.GioRa.Value - existingRecord.GioVao.Value).TotalHours;
                }
                existingRecord.SoGioLam = Math.Round(duration, 1);
                existingRecord.TrangThai = duration >= 7.5 ? "Đi làm" : "Nửa ngày";
                existingRecord.GhiChu = (existingRecord.GhiChu ?? "") + " | Check-out";
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Check-out thành công lúc {now:HH:mm}! Tổng giờ làm: {existingRecord.SoGioLam}h.";
                return RedirectToAction(nameof(Index));
            }

            // CHECK-IN: Tạo bản ghi mới với GioVao giả lập
            var checkInTime = DateTime.Now;
            if (mockShiftType == "full" || mockShiftType == "half")
            {
                // Giả lập đi làm lúc 8h sáng
                checkInTime = DateTime.Today.AddHours(8).AddMinutes(new Random().Next(-15, 10));
            }

            var record = new ChamCong
            {
                MaNV = currentUser ?? "Ẩn danh",
                NgayChamCong = DateTime.Today, // Bắt buộc dùng Today để đồng bộ Date
                GioVao = checkInTime,
                GioRa = null,
                SoGioLam = 0,
                TrangThai = "Chờ Check-out",
                GhiChu = ghiChu
            };

            _context.ChamCongs.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Check-in thành công lúc {checkInTime:HH:mm}! Nhớ quay lại Check-out cuối ca nhé.";
            return RedirectToAction(nameof(Index));
        }
    }
}