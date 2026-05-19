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
    public class PhanCongController : Controller
    {
        private readonly AppDbContext _context;
        public PhanCongController(AppDbContext context) { _context = context; }

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
            task.TrangThai = trangThai;
            _context.PhanCongs.Update(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}