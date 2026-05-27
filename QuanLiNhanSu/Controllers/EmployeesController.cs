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
    // 1. ĐÃ XÓA QUYỀN GUEST, CHỈ CÒN ADMIN VÀ EMPLOYEE
    [Authorize(Roles = "Admin,Employee")]
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
            var employees = _context.Employees.OrderBy(e => e.MaNV).AsQueryable();

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

        // ================= 2. XEM CHI TIẾT =================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // ================= 3. HỒ SƠ CÁ NHÂN (EMPLOYEE TỰ KHAI BÁO) =================
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = User.Identity!.Name;
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.MaNV == currentUser || e.HoTen == currentUser);

            if (employee == null)
            {
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
                if (avatarFile != null) model.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Add(model);
                TempData["Success"] = "Chào mừng! Hồ sơ nhân sự của bạn đã được khởi tạo.";
            }
            else
            {
                dbEntry.MaNV = model.MaNV;
                dbEntry.HoTen = model.HoTen;
                dbEntry.PhongBan = model.PhongBan;
                dbEntry.ChucVu = model.ChucVu;
                if (User.IsInRole("Admin")) dbEntry.Luong = model.Luong;

                if (avatarFile != null) dbEntry.AvatarUrl = await SaveFile(avatarFile);
                _context.Employees.Update(dbEntry);
                TempData["Success"] = "Thông tin cá nhân đã được cập nhật!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyProfile");
        }

        // ================= 4. CẬP NHẬT =================
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
                employee.Luong = original.Luong;
            }

            if (ModelState.IsValid)
            {
                if (avatarFile != null) employee.AvatarUrl = await SaveFile(avatarFile);
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ================= 5. THÊM MỚI NHÂN VIÊN & TẠO TÀI KHOẢN TỰ ĐỘNG =================
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

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
                    return View(employee);
                }

                if (avatarFile != null && avatarFile.Length > 0)
                {
                    employee.AvatarUrl = await SaveFile(avatarFile);
                }

                // 1. Lưu thông tin hồ sơ nhân viên
                _context.Add(employee);
                await _context.SaveChangesAsync();

                // 2. Tự động tạo tài khoản đăng nhập cho nhân viên này
                // (Tên đăng nhập = Mã NV, Mật khẩu = 123)
                var newAccount = new User
                {
                    Username = employee.MaNV,
                    Password = "123", // Mật khẩu mặc định chuẩn doanh nghiệp
                    Role = "Employee", // Ép cứng quyền nhân viên thường
                };

                _context.Users.Add(newAccount);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Thêm thành công! Tài khoản: {employee.MaNV} | Mật khẩu: 123";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ================= 6. XÓA NHÂN VIÊN =================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
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
                // Xóa luôn tài khoản đăng nhập gắn với nhân viên này (Nếu có)
                var linkedAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username == employee.MaNV);
                if (linkedAccount != null)
                {
                    _context.Users.Remove(linkedAccount);
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa nhân viên và tài khoản liên kết thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ================= 7. XUẤT EXCEL =================
        public async Task<IActionResult> ExportExcel()
        {
            var employees = await _context.Employees.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("NhanSu");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Mã NV";
                worksheet.Cell(currentRow, 2).Value = "Họ và Tên";
                worksheet.Cell(currentRow, 3).Value = "Phòng Ban";
                worksheet.Cell(currentRow, 4).Value = "Chức Vụ";
                worksheet.Cell(currentRow, 5).Value = "Mức Lương (VNĐ)";

                worksheet.Range("A1:E1").Style.Font.Bold = true;
                worksheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.FromHtml("#fde68a");

                foreach (var emp in employees)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = emp.MaNV;
                    worksheet.Cell(currentRow, 2).Value = emp.HoTen;
                    worksheet.Cell(currentRow, 3).Value = emp.PhongBan;
                    worksheet.Cell(currentRow, 4).Value = emp.ChucVu;
                    worksheet.Cell(currentRow, 5).Value = emp.Luong;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachNhanSu.xlsx");
                }
            }
        }

        // ================= HÀM HỖ TRỢ LƯU FILE =================
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