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
    public class NghiPhepController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _audit;

        public NghiPhepController(AppDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity!.Name;
            var records = _context.PhieuNghiPheps.AsQueryable();

            if (!User.IsInRole("Admin") && !User.IsInRole("Nhân sự"))
            {
                records = records.Where(x => x.MaNV == currentUser);
            }

            return View(await records.OrderByDescending(x => x.NgayBatDau).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuNghiPhep model)
        {
            if (model.NgayKetThuc < model.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                // Mặc định gán mã NV hiện tại (Employee tự xin nghỉ)
                if (!User.IsInRole("Admin") && !User.IsInRole("Nhân sự"))
                {
                    model.MaNV = User.Identity!.Name!;
                }
                
                model.TrangThai = "Chờ duyệt";
                _context.PhieuNghiPheps.Add(model);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, "Tạo đơn nghỉ phép", "NghiPhep",
                    $"NV {model.MaNV} xin nghỉ từ {model.NgayBatDau:dd/MM} đến {model.NgayKetThuc:dd/MM}.");

                TempData["Success"] = "Đã gửi đơn xin nghỉ phép! Vui lòng chờ phê duyệt.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nhân sự")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string actionType)
        {
            var phieu = await _context.PhieuNghiPheps.FindAsync(id);
            if (phieu == null) return NotFound();

            var approver = User.Identity!.Name!;
            
            if (actionType == "Approve")
            {
                phieu.TrangThai = "Đã duyệt";
                await _audit.LogAsync(approver, "Duyệt nghỉ phép", "NghiPhep",
                    $"[{approver}] ĐÃ DUYỆT đơn xin nghỉ của NV {phieu.MaNV}.");
                TempData["Success"] = $"Đã duyệt đơn nghỉ phép của {phieu.MaNV}!";
            }
            else if (actionType == "Reject")
            {
                phieu.TrangThai = "Từ chối";
                await _audit.LogAsync(approver, "Từ chối nghỉ phép", "NghiPhep",
                    $"[{approver}] TỪ CHỐI đơn xin nghỉ của NV {phieu.MaNV}.");
                TempData["Success"] = $"Đã từ chối đơn nghỉ phép của {phieu.MaNV}.";
            }

            _context.PhieuNghiPheps.Update(phieu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
