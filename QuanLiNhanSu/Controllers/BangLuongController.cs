using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin,KeToan")]
    public class BangLuongController : Controller
    {
        private readonly AppDbContext _context;
        public BangLuongController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index() => View(await _context.BangLuongs.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> ChotLuongThang(int thang, int nam)
        {
            var emps = await _context.Employees.ToListAsync();

            foreach (var e in emps)
            {
                // Đếm số ngày công chuẩn: 1 ngày công "Đi làm" tính hệ số 1.0, ngày "Nửa ngày" tính hệ số 0.5
                var fullDays = await _context.ChamCongs.CountAsync(c => c.MaNV == e.MaNV && c.NgayChamCong.Month == thang && c.TrangThai == "Đi làm");
                var halfDays = await _context.ChamCongs.CountAsync(c => c.MaNV == e.MaNV && c.NgayChamCong.Month == thang && c.TrangThai == "Nửa ngày");

                double totalWorkDays = fullDays + (halfDays * 0.5);

                // Tính toán tiền tạm ứng lương đã được duyệt
                var advanced = await _context.UngLuongs
                    .Where(u => u.MaNV == e.MaNV && u.NgayYeuCau.Month == thang && u.TrangThai == "Đã duyệt")
                    .SumAsync(u => u.SoTien);

                decimal realSalary = totalWorkDays > 0 ? (e.Luong / 26 * (decimal)totalWorkDays) - advanced : 0;

                _context.BangLuongs.Add(new BangLuong
                {
                    MaNV = e.MaNV,
                    Thang = thang,
                    Nam = nam,
                    LuongCoBan = e.Luong,
                    TienUng = advanced,
                    ThucLanh = realSalary < 0 ? 0 : realSalary
                });
            }
            await _context.SaveChangesAsync();
            // Ghi nhận Audit Log
            _context.SystemLogs.Add(new SystemLog
            {
                Username = User.Identity!.Name ?? "System",
                Action = "Chốt Lương Tự Động",
                Target = "BangLuongs",
                Details = $"Đã chốt lương tháng {thang}/{nam} cho toàn bộ nhân sự."
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã tính toán và chốt sổ lương tự động tháng này!";
            return RedirectToAction(nameof(Index));
        }
    }
}