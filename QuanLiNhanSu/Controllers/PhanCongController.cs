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
    public class PhanCongController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _audit;

        public PhanCongController(AppDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var user = User.Identity!.Name;
            var tasks = _context.PhanCongs.AsQueryable();
            if (!User.IsInRole("Admin")) tasks = tasks.Where(t => t.MaNV == user);
            return View(await tasks.OrderByDescending(t => t.HanChot).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GiaoViec(PhanCong model)
        {
            if (ModelState.IsValid)
            {
                model.TrangThai = "Mới giao";
                _context.PhanCongs.Add(model);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, "Giao việc", "PhanCongs",
                    $"Admin giao việc [{model.TenCongViec}] cho NV {model.MaNV}. Hạn chót: {model.HanChot:dd/MM/yyyy}.");

                TempData["Success"] = "Đã giao việc thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTienDo(int id, string trangThai)
        {
            var task = await _context.PhanCongs.FindAsync(id);
            if (task == null) return NotFound();

            var oldStatus = task.TrangThai;
            task.TrangThai = trangThai;
            _context.PhanCongs.Update(task);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(User.Identity!.Name!, "Cập nhật tiến độ", "PhanCongs",
                $"NV {task.MaNV} cập nhật việc [{task.TenCongViec}]: {oldStatus} → {trangThai}.");

            return RedirectToAction(nameof(Index));
        }
    }
}