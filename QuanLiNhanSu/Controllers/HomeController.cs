using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;
using System.Linq;
using System.Threading.Tasks;

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
            // TỐI ƯU HÓA ĐỈNH CAO: Kéo dữ liệu về RAM một lần duy nhất để giải phóng kết nối DB ngay lập tức.
            // Khắc phục hoàn toàn lỗi kẹt luồng "Connection Timeout Expired" ở Post-Login.
            var employees = await _context.Employees.ToListAsync();

            // 1. Lấy tổng số lượng và quỹ lương cực nhanh từ danh sách trên RAM
            ViewBag.TotalEmployees = employees.Count;
            ViewBag.TotalSalary = employees.Sum(e => e.Luong);

            // 2. Gom nhóm theo Phòng ban bằng LINQ to Objects trên RAM để vẽ Biểu đồ
            var stats = employees
                .GroupBy(e => string.IsNullOrEmpty(e.PhongBan) ? "Chưa phân phòng" : e.PhongBan)
                .Select(g => new {
                    Department = g.Key,
                    Count = g.Count(),
                    SalaryFund = g.Sum(e => e.Luong)
                }).ToList();

            // Truyền mảng dữ liệu sang View
            ViewBag.Labels = stats.Select(s => s.Department).ToList();
            ViewBag.Counts = stats.Select(s => s.Count).ToList();
            ViewBag.Salaries = stats.Select(s => s.SalaryFund).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}