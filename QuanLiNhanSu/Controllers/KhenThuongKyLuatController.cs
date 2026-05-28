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
    [Authorize]
    public class KhenThuongKyLuatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _audit;

        public KhenThuongKyLuatController(AppDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var records = _context.KhenThuongKyLuats.AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("KeToan") && !User.IsInRole("Nhân sự"))
            {
                records = records.Where(x => x.MaNV == currentUser);
            }

            return View(await records.OrderByDescending(x => x.NgayQuyetDinh).ToListAsync());
        }

        [Authorize(Roles = "Admin,Nhân sự")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhenThuongKyLuat model)
        {
            if (ModelState.IsValid)
            {
                _context.KhenThuongKyLuats.Add(model);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, $"Tạo {model.Loai}", "KhenThuongKyLuat",
                    $"Đã tạo quyết định {model.Loai.ToLower()} ({model.HinhThuc}) cho NV {model.MaNV}. Số tiền: {model.SoTien:N0} VNĐ.");

                TempData["Success"] = $"Đã tạo quyết định {model.Loai.ToLower()} thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.KhenThuongKyLuats.FindAsync(id);
            if (record != null)
            {
                _context.KhenThuongKyLuats.Remove(record);
                await _context.SaveChangesAsync();
                
                await _audit.LogAsync(User.Identity!.Name!, $"Xóa {record.Loai}", "KhenThuongKyLuat",
                    $"Đã xóa quyết định {record.Loai.ToLower()} mã {id} của NV {record.MaNV}.");
                    
                TempData["Success"] = "Đã xóa quyết định thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
