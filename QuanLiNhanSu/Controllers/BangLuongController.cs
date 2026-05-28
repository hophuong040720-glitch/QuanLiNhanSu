using System;
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

        public async Task<IActionResult> Index()
        {
            var sheets = await _context.BangLuongs.OrderByDescending(b => b.Nam).ThenByDescending(b => b.Thang).ThenBy(b => b.MaNV).ToListAsync();
            return View(sheets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Đảm bảo khớp cấu trúc bảo mật Token từ Form gửi lên
        public async Task<IActionResult> ChotLuongThang(int thang, int nam)
        {
            var employees = await _context.Employees.ToListAsync();

            foreach (var emp in employees)
            {
                // 1. Tính toán số ngày công dựa trên dữ liệu chấm công thực tế của tháng/năm
                var fullDays = await _context.ChamCongs.CountAsync(c => c.MaNV == emp.MaNV && c.NgayChamCong.Month == thang && c.NgayChamCong.Year == nam && c.TrangThai == "Đi làm");
                var halfDays = await _context.ChamCongs.CountAsync(c => c.MaNV == emp.MaNV && c.NgayChamCong.Month == thang && c.NgayChamCong.Year == nam && c.TrangThai == "Nửa ngày");

                double currentWorkDays = fullDays + (halfDays * 0.5);

                // 2. Lấy tổng tiền đã ứng lương tạm thời trong tháng
                decimal advancedAmount = await _context.UngLuongs
                    .Where(u => u.MaNV == emp.MaNV && u.NgayYeuCau.Month == thang && u.NgayYeuCau.Year == nam && u.TrangThai == "Đã duyệt")
                    .SumAsync(u => u.SoTien);

                // 2.1. Lấy tổng tiền Thưởng và tiền Phạt trong tháng
                decimal tienThuong = await _context.KhenThuongKyLuats
                    .Where(k => k.MaNV == emp.MaNV && k.Loai == "Khen thưởng" && k.NgayQuyetDinh.Month == thang && k.NgayQuyetDinh.Year == nam)
                    .SumAsync(k => k.SoTien);
                decimal tienPhat = await _context.KhenThuongKyLuats
                    .Where(k => k.MaNV == emp.MaNV && k.Loai == "Kỷ luật" && k.NgayQuyetDinh.Month == thang && k.NgayQuyetDinh.Year == nam)
                    .SumAsync(k => k.SoTien);

                // 3. Tính toán tiền lương phát sinh của lượt chốt này
                decimal calculatedSalary = currentWorkDays > 0 ? (emp.Luong / 26m * (decimal)currentWorkDays) + tienThuong - tienPhat - advancedAmount : 0;
                decimal roundedSalary = Math.Round(calculatedSalary, 0);
                if (roundedSalary < 0) roundedSalary = 0;

                // 4. KIỂM TRA BẢN GHI CŨ (LOGIC CỘNG DỒN VÀ XÓA CŨ DÀNH CHỖ CHO LƯƠNG MỚI)
                var oldRecord = await _context.BangLuongs.FirstOrDefaultAsync(b => b.MaNV == emp.MaNV && b.Thang == thang && b.Nam == nam);

                int finalWorkDays = (int)Math.Floor(currentWorkDays);
                decimal finalThucLanh = roundedSalary;

                if (oldRecord != null)
                {
                    // Nếu đã có lương tháng này: Cộng dồn ngày làm và cộng gộp tiền thực lãnh cũ vào mới
                    finalWorkDays += oldRecord.SoNgayDiLam;
                    finalThucLanh += oldRecord.ThucLanh;

                    // Xóa dòng cũ đi để chuẩn bị ghi đè dữ liệu mới tổng hợp
                    _context.BangLuongs.Remove(oldRecord);
                    await _context.SaveChangesAsync();
                }

                // 5. Thêm bản ghi lương mới hoàn chỉnh vào hệ thống
                _context.BangLuongs.Add(new BangLuong
                {
                    MaNV = emp.MaNV,
                    Thang = thang,
                    Nam = nam,
                    LuongCoBan = emp.Luong,
                    SoNgayDiLam = finalWorkDays,
                    TienUng = advancedAmount,
                    TienThuong = tienThuong,
                    TienPhat = tienPhat,
                    ThucLanh = finalThucLanh
                });

                // 6. Ghi vết hệ thống Audit Log
                _context.SystemLogs.Add(new SystemLog
                {
                    Username = User.Identity!.Name ?? "Admin",
                    Action = "Chốt Lương",
                    Target = "BangLuong",
                    Timestamp = DateTime.Now,
                    Details = $"Chốt lương nhân viên {emp.MaNV} - Tháng {thang}/{nam}. Tổng công tích lũy: {finalWorkDays} ngày."
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Hệ thống đã chốt sổ và tổng hợp lương tháng {thang}/{nam} thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}