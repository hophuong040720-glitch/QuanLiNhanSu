using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuanLiNhanSu.Models;
using QuanLiNhanSu.Services;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuditService _audit;
        private readonly PasswordService _pwdService;

        public EmployeesController(AppDbContext context, IWebHostEnvironment webHostEnvironment, AuditService audit, PasswordService pwdService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _audit = audit;
            _pwdService = pwdService;
        }

        // ================= HELPER: Load ViewBag dropdowns =================
        private async Task LoadDropdownsAsync(int? selectedPhongBanId = null, int? selectedChucVuId = null)
        {
            ViewBag.PhongBans = new SelectList(await _context.PhongBans.OrderBy(p => p.TenPB).ToListAsync(), "MaPB", "TenPB", selectedPhongBanId);
            ViewBag.ChucVus = new SelectList(await _context.ChucVus.OrderBy(c => c.TenCV).ToListAsync(), "MaCV", "TenCV", selectedChucVuId);
        }

        // ================= 1. DANH SÁCH CHUNG =================
        public async Task<IActionResult> Index(string searchString, int? phongBanId, int page = 1)
        {
            int pageSize = 5;
            var employees = _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .OrderBy(e => e.MaNV)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                employees = employees.Where(e => e.HoTen.Contains(searchString) || e.MaNV.Contains(searchString));

            if (phongBanId.HasValue)
                employees = employees.Where(e => e.PhongBanId == phongBanId.Value);

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPhongBanId = phongBanId;
            ViewBag.PhongBans = await _context.PhongBans.OrderBy(p => p.TenPB).ToListAsync();

            int totalItems = await employees.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = page < 1 ? 1 : page;
            page = page > totalPages && totalPages > 0 ? totalPages : page;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(await employees.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        // ================= 2. XEM CHI TIẾT =================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // ================= 3. HỒ SƠ CÁ NHÂN =================
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = User.Identity!.Name;
            var employee = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .FirstOrDefaultAsync(e => e.MaNV == currentUser || e.HoTen == currentUser);

            await LoadDropdownsAsync(employee?.PhongBanId, employee?.ChucVuId);

            if (employee == null)
            {
                return View(new Employee { MaNV = currentUser!, HoTen = currentUser! });
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
                if (avatarFile != null) model.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Add(model);
                await _audit.LogAsync(currentUser!, "Tạo hồ sơ", "Employees", $"Nhân viên {currentUser} tự tạo hồ sơ cá nhân.");
                TempData["Success"] = "Chào mừng! Hồ sơ nhân sự của bạn đã được khởi tạo.";
            }
            else
            {
                dbEntry.MaNV = model.MaNV;
                dbEntry.HoTen = model.HoTen;
                dbEntry.PhongBanId = model.PhongBanId;
                dbEntry.ChucVuId = model.ChucVuId;
                if (User.IsInRole("Admin")) dbEntry.Luong = model.Luong;
                if (avatarFile != null) dbEntry.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Update(dbEntry);
                await _audit.LogAsync(currentUser!, "Cập nhật hồ sơ", "Employees", $"Nhân viên {currentUser} cập nhật hồ sơ cá nhân.");
                TempData["Success"] = "Thông tin cá nhân đã được cập nhật!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyProfile");
        }

        // ================= 4. CẬP NHẬT (Admin) =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            await LoadDropdownsAsync(employee.PhongBanId, employee.ChucVuId);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? avatarFile)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var original = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                if (avatarFile != null)
                    employee.AvatarUrl = await SaveFile(avatarFile);
                else
                    employee.AvatarUrl = original?.AvatarUrl;

                _context.Update(employee);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, "Sửa nhân viên", "Employees",
                    $"Admin sửa hồ sơ NV: {employee.MaNV} - {employee.HoTen}. Lương: {original?.Luong:N0} → {employee.Luong:N0} VNĐ.");

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdownsAsync(employee.PhongBanId, employee.ChucVuId);
            return View(employee);
        }

        // ================= 5. THÊM MỚI =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Employee employee, IFormFile? avatarFile)
        {
            if (ModelState.IsValid)
            {
                if (_context.Employees.Any(e => e.MaNV == employee.MaNV))
                {
                    ModelState.AddModelError("MaNV", "Mã nhân viên đã tồn tại.");
                    await LoadDropdownsAsync(employee.PhongBanId, employee.ChucVuId);
                    return View(employee);
                }

                if (avatarFile != null && avatarFile.Length > 0)
                    employee.AvatarUrl = await SaveFile(avatarFile);

                _context.Add(employee);
                await _context.SaveChangesAsync();

                // Tự động tạo tài khoản đăng nhập (mật khẩu đã được hash)
                var defaultPassword = "123456";
                var newAccount = new User
                {
                    Username = employee.MaNV,
                    Password = _pwdService.HashPassword(defaultPassword),
                    Role = "Employee"
                };
                _context.Users.Add(newAccount);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(User.Identity!.Name!, "Thêm nhân viên", "Employees",
                    $"Thêm mới NV: {employee.MaNV} - {employee.HoTen}. Tài khoản tự động: {employee.MaNV} / {defaultPassword}.");

                TempData["Success"] = $"Thêm thành công! Tài khoản: {employee.MaNV} | Mật khẩu mặc định: {defaultPassword}";
                return RedirectToAction(nameof(Index));
            }
            await LoadDropdownsAsync(employee.PhongBanId, employee.ChucVuId);
            return View(employee);
        }

        // ================= 6. XÓA =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                var linkedAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username == employee.MaNV);
                if (linkedAccount != null)
                    _context.Users.Remove(linkedAccount);

                await _audit.LogAsync(User.Identity!.Name!, "Xóa nhân viên", "Employees",
                    $"Xóa nhân viên: {employee.MaNV} - {employee.HoTen}. Tài khoản liên kết đã bị xóa theo.");

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa nhân viên và tài khoản liên kết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ================= 7. XUẤT EXCEL =================
        public async Task<IActionResult> ExportExcel()
        {
            var employees = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("NhanSu");
            var currentRow = 1;

            // Header
            worksheet.Cell(currentRow, 1).Value = "Mã NV";
            worksheet.Cell(currentRow, 2).Value = "Họ và Tên";
            worksheet.Cell(currentRow, 3).Value = "Phòng Ban";
            worksheet.Cell(currentRow, 4).Value = "Chức Vụ";
            worksheet.Cell(currentRow, 5).Value = "Mức Lương (VNĐ)";

            worksheet.Range("A1:E1").Style.Font.Bold = true;
            worksheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.FromHtml("#4f46e5");
            worksheet.Range("A1:E1").Style.Font.FontColor = XLColor.White;

            foreach (var emp in employees)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = emp.MaNV;
                worksheet.Cell(currentRow, 2).Value = emp.HoTen;
                worksheet.Cell(currentRow, 3).Value = emp.TenPhongBan;
                worksheet.Cell(currentRow, 4).Value = emp.TenChucVu;
                worksheet.Cell(currentRow, 5).Value = emp.Luong;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachNhanSu.xlsx");
        }

        // ================= HÀM HỖ TRỢ =================
        private async Task<string> SaveFile(IFormFile file)
        {
            string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            using var fs = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(fs);
            return "/uploads/" + fileName;
        }

        private bool EmployeeExists(int id) => _context.Employees.Any(e => e.Id == id);
    }
}