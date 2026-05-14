using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuanLiNhanSu.Models;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin,Employee,Guest")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeesController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ================= 1. DANH SÁCH CHUNG =================
        public async Task<IActionResult> Index(string searchString, string phongBan, int page = 1)
        {
            int pageSize = 5;
            var employees = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                employees = employees.Where(e => e.HoTen.Contains(searchString) || e.MaNV.Contains(searchString));

            if (!string.IsNullOrEmpty(phongBan))
                employees = employees.Where(e => e.PhongBan == phongBan);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPhongBan = phongBan;
            ViewBag.PhongBans = await _context.Employees.Select(e => e.PhongBan).Distinct().ToListAsync();

            int totalItems = await employees.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = page < 1 ? 1 : page;
            page = page > totalPages && totalPages > 0 ? totalPages : page;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(await employees.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // ================= 2. HỒ SƠ CÁ NHÂN (EMPLOYEE TỰ KHAI BÁO) =================
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = User.Identity!.Name;
            // Tìm theo Mã NV hoặc Họ tên trùng với Username đăng ký
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.MaNV == currentUser || e.HoTen == currentUser);

            if (employee == null)
            {
                // Nếu chưa có trong bảng Employee, khởi tạo mẫu để họ tự điền
                return View(new Employee { MaNV = currentUser, HoTen = currentUser, PhongBan = "", ChucVu = "", Luong = 0 });
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Employee model, IFormFile? avatarFile)
        {
            var currentUser = User.Identity!.Name;
            var dbEntry = await _context.Employees.FirstOrDefaultAsync(e => e.Id == model.Id || e.MaNV == currentUser);

            if (dbEntry == null)
            {
                // TẠO MỚI (Dữ liệu chạy thẳng vào bảng database)
                if (avatarFile != null) model.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Add(model);
                TempData["Success"] = "Chào mừng! Hồ sơ nhân sự của bạn đã được khởi tạo.";
            }
            else
            {
                // CẬP NHẬT
                dbEntry.MaNV = model.MaNV;
                dbEntry.HoTen = model.HoTen;
                dbEntry.PhongBan = model.PhongBan;
                dbEntry.ChucVu = model.ChucVu;
                if (User.IsInRole("Admin")) dbEntry.Luong = model.Luong; // Chỉ Admin mới ghi đè lương ở đây

                if (avatarFile != null) dbEntry.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Update(dbEntry);
                TempData["Success"] = "Thông tin cá nhân đã được cập nhật!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyProfile");
        }

        // ================= 3. CẬP NHẬT (ADMIN HOẶC CHỦ SỞ HỮU) =================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            if (User.IsInRole("Employee"))
            {
                var user = User.Identity!.Name;
                if (employee.MaNV != user && !employee.HoTen.Contains(user))
                    return RedirectToAction("AccessDenied", "Account");
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? avatarFile)
        {
            if (id != employee.Id) return NotFound();

            if (User.IsInRole("Employee"))
            {
                var original = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                employee.Luong = original.Luong; // Không cho Employee tự nâng lương
            }

            if (ModelState.IsValid)
            {
                if (avatarFile != null) employee.AvatarUrl = await SaveFile(avatarFile);
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ================= 4. CÁC QUYỀN CỦA ADMIN =================
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id) => View(await _context.Employees.FindAsync(id));

        private async Task<string> SaveFile(IFormFile file)
        {
            string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            using (var fs = new FileStream(Path.Combine(folder, fileName), FileMode.Create)) { await file.CopyToAsync(fs); }
            return "/uploads/" + fileName;
        }

        private bool EmployeeExists(int id) => _context.Employees.Any(e => e.Id == id);
    }
}