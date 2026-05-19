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
                var days = await _context.ChamCongs.CountAsync(c => c.MaNV == e.MaNV && c.NgayChamCong.Month == thang && c.TrangThai == "Đi làm");
                var advanced = await _context.UngLuongs.Where(u => u.MaNV == e.MaNV && u.NgayYeuCau.Month == thang && u.TrangThai == "Đã duyệt").SumAsync(u => u.SoTien);
                _context.BangLuongs.Add(new BangLuong { MaNV = e.MaNV, Thang = thang, Nam = nam, ThucLanh = (e.Luong / 26 * days) - advanced });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}