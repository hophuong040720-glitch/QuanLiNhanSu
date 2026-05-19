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
    public class UngLuongController : Controller
    {
        private readonly AppDbContext _context;

        public UngLuongController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UngLuong (Admin/Kế toán thấy hết, Employee chỉ thấy phiếu của mình)
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var requests = _context.UngLuongs.AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("KeToan"))
            {
                requests = requests.Where(u => u.MaNV == currentUser);
            }

            return View(await requests.OrderByDescending(u => u.NgayYeuCau).ToListAsync());
        }

        // POST: UngLuong/CreateRequest (Nhân viên gửi yêu cầu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(decimal soTien, string lyDo)
        {
            if (soTien <= 0)
            {
                TempData["Error"] = "Số tiền ứng phải lớn hơn 0đ!";
                return RedirectToAction(nameof(Index));
            }

            var record = new UngLuong
            {
                MaNV = User.Identity!.Name ?? "Ẩn danh",
                NgayYeuCau = DateTime.Now,
                SoTien = soTien,
                LyDo = lyDo,
                TrangThai = "Chờ duyệt"
            };

            _context.UngLuongs.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gửi yêu cầu ứng lương thành công! Vui lòng chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        // POST: UngLuong/Approve (Chỉ Admin hoặc Kế toán được duyệt)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,KeToan")]
        public async Task<IActionResult> Approve(int id, string actionType)
        {
            var request = await _context.UngLuongs.FindAsync(id);
            if (request == null) return NotFound();

            if (actionType == "Approve")
            {
                request.TrangThai = "Đã duyệt";
                TempData["Success"] = $"Đã phê duyệt phiếu ứng lương của nhân viên {request.MaNV}!";
            }
            else if (actionType == "Reject")
            {
                request.TrangThai = "Từ chối";
                TempData["Success"] = $"Đã từ chối phiếu ứng lương của nhân viên {request.MaNV}.";
            }

            _context.UngLuongs.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}