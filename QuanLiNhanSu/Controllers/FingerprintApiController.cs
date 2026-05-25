using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using QuanLiNhanSu.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLiNhanSu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FingerprintController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FingerprintController(AppDbContext context) { _context = context; }

        public class FingerprintData
        {
            public string DeviceId { get; set; }
            public string MaNV { get; set; }
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ReceiveScanData([FromBody] FingerprintData data)
        {
            if (string.IsNullOrEmpty(data.MaNV))
                return BadRequest(new { status = "error", message = "Thiếu mã nhân viên!" });

            var today = DateTime.Today;

            // Tìm xem hôm nay nhân viên này đã quét vân tay lần nào chưa
            var attendanceToday = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNV == data.MaNV && c.NgayChamCong.Date == today);

            if (attendanceToday == null)
            {
                // LẦN 1: Tiến hành Check-in đầu ca làm việc
                var newRecord = new ChamCong
                {
                    MaNV = data.MaNV,
                    NgayChamCong = today,
                    GioVao = DateTime.Now,
                    TrangThai = "Chờ Check-out",
                    GhiChu = $"Quét Check-in từ máy {data.DeviceId}"
                };
                _context.ChamCongs.Add(newRecord);
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", action = "CheckIn", message = "Ghi nhận giờ vào thành công!" });
            }
            else if (attendanceToday.GioRa == null)
            {
                // LẦN 2: Tiến hành Check-out cuối ca làm việc
                attendanceToday.GioRa = DateTime.Now;

                // Thuật toán tự động tính tổng thời gian làm việc
                TimeSpan duration = attendanceToday.GioRa.Value - attendanceToday.GioVao.Value;
                attendanceToday.SoGioLam = Math.Round(duration.TotalHours, 2);

                // Quy định doanh nghiệp thực tế: Làm trên 7.5 tiếng tính đủ 1 ngày công, dưới 7.5 tiếng tính nửa ca
                if (attendanceToday.SoGioLam >= 7.5)
                {
                    attendanceToday.TrangThai = "Đi làm";
                }
                else
                {
                    attendanceToday.TrangThai = "Nửa ngày";
                }

                attendanceToday.GhiChu += $" | Check-out lúc {DateTime.Now:HH:mm:ss}";
                _context.ChamCongs.Update(attendanceToday);
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", action = "CheckOut", message = $"Ghi nhận giờ ra thành công! Tổng giờ: {attendanceToday.SoGioLam} tiếng." });
            }

            return Ok(new { status = "warning", message = "Nhân viên này đã hoàn thành chấm công ra vào trong ngày!" });
        }
    }
}