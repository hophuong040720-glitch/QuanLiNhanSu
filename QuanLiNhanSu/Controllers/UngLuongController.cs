using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;
using QuanLiNhanSu.Services;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class UngLuongController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _audit;

        public UngLuongController(AppDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var requests = _context.UngLuongs.AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("KeToan"))
                requests = requests.Where(u => u.MaNV == currentUser);

            return View(await requests.OrderByDescending(u => u.NgayYeuCau).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(decimal soTien, string lyDo)
        {
            if (soTien <= 0)
            {
                TempData["Error"] = "Số tiền ứng phải lớn hơn 0đ!";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = User.Identity!.Name ?? "Ẩn danh";
            var record = new UngLuong
            {
                MaNV = currentUser,
                NgayYeuCau = DateTime.Now,
                SoTien = soTien,
                LyDo = lyDo,
                TrangThai = "Chờ duyệt"
            };

            _context.UngLuongs.Add(record);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(currentUser, "Gửi ứng lương", "UngLuongs",
                $"NV [{currentUser}] gửi yêu cầu ứng lương {soTien:N0} VNĐ. Lý do: {lyDo}.");

            TempData["Success"] = "Gửi yêu cầu ứng lương thành công! Vui lòng chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,KeToan")]
        public async Task<IActionResult> Approve(int id, string actionType)
        {
            var request = await _context.UngLuongs.FindAsync(id);
            if (request == null) return NotFound();

            var approver = User.Identity!.Name!;
            if (actionType == "Approve")
            {
                request.TrangThai = "Đã duyệt";
                await _audit.LogAsync(approver, "Duyệt ứng lương", "UngLuongs",
                    $"[{approver}] ĐÃ DUYỆT phiếu ứng lương của NV {request.MaNV}: {request.SoTien:N0} VNĐ.");
                TempData["Success"] = $"Đã phê duyệt phiếu ứng lương của {request.MaNV}!";
            }
            else if (actionType == "Reject")
            {
                request.TrangThai = "Từ chối";
                await _audit.LogAsync(approver, "Từ chối ứng lương", "UngLuongs",
                    $"[{approver}] TỪ CHỐI phiếu ứng lương của NV {request.MaNV}: {request.SoTien:N0} VNĐ.");
                TempData["Success"] = $"Đã từ chối phiếu ứng lương của {request.MaNV}.";
            }

            _context.UngLuongs.Update(request);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}