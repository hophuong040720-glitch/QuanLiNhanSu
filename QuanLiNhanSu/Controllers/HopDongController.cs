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
    public class HopDongController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _audit;

        public HopDongController(AppDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var hopDongs = _context.HopDongs.AsQueryable();

            // Nếu không phải Admin/KeToan/NhanSu thì chỉ xem được hợp đồng của chính mình
            if (!User.IsInRole("Admin") && !User.IsInRole("KeToan") && !User.IsInRole("Nhân sự"))
            {
                hopDongs = hopDongs.Where(h => h.MaNV == currentUser);
            }

            return View(await hopDongs.OrderByDescending(h => h.NgayKy).ToListAsync());
        }

        [Authorize(Roles = "Admin,Nhân sự")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong model)
        {
            if (ModelState.IsValid)
            {
                _context.HopDongs.Add(model);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, "Tạo Hợp Đồng", "HopDongs",
                    $"Đã tạo hợp đồng mới [{model.LoaiHD}] cho nhân viên {model.MaNV}.");

                TempData["Success"] = "Đã thêm hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin,Nhân sự")]
        public async Task<IActionResult> Edit(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null)
            {
                return NotFound();
            }
            return View(hopDong);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HopDong model)
        {
            if (id != model.MaHD)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    await _audit.LogAsync(User.Identity!.Name!, "Cập nhật Hợp Đồng", "HopDongs",
                        $"Đã cập nhật hợp đồng mã {model.MaHD} của NV {model.MaNV}.");

                    TempData["Success"] = "Cập nhật hợp đồng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HopDongExists(model.MaHD))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong != null)
            {
                _context.HopDongs.Remove(hopDong);
                await _context.SaveChangesAsync();
                
                await _audit.LogAsync(User.Identity!.Name!, "Xóa Hợp Đồng", "HopDongs",
                    $"Đã xóa hợp đồng mã {id} của NV {hopDong.MaNV}.");
                    
                TempData["Success"] = "Đã xóa hợp đồng!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HopDongExists(int id)
        {
            return _context.HopDongs.Any(e => e.MaHD == id);
        }
    }
}
