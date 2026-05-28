using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace QuanLiNhanSu.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Load employees với navigation properties để dùng TenPhongBan
            var employees = await _context.Employees
                .Include(e => e.PhongBanNav)
                .Include(e => e.ChucVuNav)
                .ToListAsync();

            // Thống kê cơ bản
            ViewBag.TotalEmployees = employees.Count;
            ViewBag.TotalSalary = employees.Sum(e => e.Luong);

            // Thống kê theo Phòng ban (dùng TenPhongBan thay vì chuỗi cũ)
            var stats = employees
                .GroupBy(e => e.TenPhongBan)
                .Select(g => new {
                    Department = g.Key,
                    Count = g.Count(),
                    SalaryFund = g.Sum(e => e.Luong)
                }).ToList();

            ViewBag.Labels = stats.Select(s => s.Department).ToList();
            ViewBag.Counts = stats.Select(s => s.Count).ToList();
            ViewBag.Salaries = stats.Select(s => s.SalaryFund).ToList();

            // ==========================================
            // THỐNG KÊ DASHBOARD NÂNG CAO (GIAI ĐOẠN 3)
            // ==========================================
            var today = DateTime.Today;

            // 1. Số nhân viên chấm công hôm nay
            ViewBag.PresentToday = await _context.ChamCongs
                .Where(c => c.NgayChamCong.Date == today)
                .Select(c => c.MaNV)
                .Distinct()
                .CountAsync();

            // 2. Số Hợp đồng sắp hết hạn (<= 30 ngày)
            var thresholdDate = today.AddDays(30);
            ViewBag.ExpiringContracts = await _context.HopDongs
                .Where(h => h.NgayHetHan != null && h.NgayHetHan.Value >= today && h.NgayHetHan.Value <= thresholdDate)
                .CountAsync();
                
            ViewBag.ExpiringContractList = await _context.HopDongs
                .Where(h => h.NgayHetHan != null && h.NgayHetHan.Value >= today && h.NgayHetHan.Value <= thresholdDate)
                .OrderBy(h => h.NgayHetHan)
                .ToListAsync();

            // 3. Đơn xin nghỉ phép đang "Chờ duyệt"
            ViewBag.PendingLeaves = await _context.PhieuNghiPheps
                .Where(p => p.TrangThai == "Chờ duyệt")
                .CountAsync();

            // 4. Số đơn ứng lương "Chờ duyệt"
            ViewBag.PendingAdvances = await _context.UngLuongs
                .Where(u => u.TrangThai == "Chờ duyệt")
                .CountAsync();

            // 5. Activity Logs gần nhất
            ViewBag.RecentLogs = await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}