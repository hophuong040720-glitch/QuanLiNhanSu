using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLiNhanSu.Controllers
{
    // Giữ Authorize chung: Bắt buộc phải đăng nhập mới được vào xem Nhân viên
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // ================= 1. XEM DANH SÁCH (Ai đăng nhập cũng xem được) =================
        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // ================= 2. XEM CHI TIẾT (Ai đăng nhập cũng xem được) =================
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // ================= 3. THÊM MỚI (CHỈ ADMIN MỚI ĐƯỢC LÀM) =================
        // GET: Employees/Create
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP GET
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP POST
        public async Task<IActionResult> Create([Bind("Id,MaNV,HoTen,ChucVu,Luong,PhongBan")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ================= 4. SỬA (CHỈ ADMIN MỚI ĐƯỢC LÀM) =================
        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP POST
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaNV,HoTen,ChucVu,Luong,PhongBan")] Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ================= 5. XÓA (CHỈ ADMIN MỚI ĐƯỢC LÀM) =================
        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KHÓA QUYỀN TRUY CẬP POST
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}