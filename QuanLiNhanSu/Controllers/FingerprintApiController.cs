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
        private readonly Services.AuditService _audit;

        public FingerprintController(AppDbContext context, Services.AuditService audit) 
        { 
            _context = context; 
            _audit = audit;
        }

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

            var empExists = await _context.Employees.AnyAsync(e => e.MaNV == data.MaNV);
            if (!empExists)
                return NotFound(new { status = "error", message = "Mã nhân viên không tồn tại trong hệ thống!" });

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
                
                await _audit.LogAsync("System", "Check-In Vân Tay", "ChamCong", $"NV {data.MaNV} check-in từ thiết bị {data.DeviceId}.");

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
                
                await _audit.LogAsync("System", "Check-Out Vân Tay", "ChamCong", $"NV {data.MaNV} check-out từ thiết bị {data.DeviceId}. Tổng giờ: {attendanceToday.SoGioLam}.");

                return Ok(new { status = "success", action = "CheckOut", message = $"Ghi nhận giờ ra thành công! Tổng giờ: {attendanceToday.SoGioLam} tiếng." });
            }

            return Ok(new { status = "warning", message = "Nhân viên này đã hoàn thành chấm công ra vào trong ngày!" });
        }

        [HttpGet("status/{maNV}")]
        public async Task<IActionResult> GetStatus(string maNV)
        {
            var today = DateTime.Today;
            var record = await _context.ChamCongs
                .FirstOrDefaultAsync(c => c.MaNV == maNV && c.NgayChamCong.Date == today);

            if (record == null)
                return Ok(new { status = "NoRecord", message = "Chưa check-in" });
            
            if (record.GioRa == null)
                return Ok(new { status = "CheckedIn", time = record.GioVao, message = "Đã check-in, chờ check-out" });

            return Ok(new { status = "Completed", timeIn = record.GioVao, timeOut = record.GioRa, totalHours = record.SoGioLam });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var today = DateTime.Today;
            var records = await _context.ChamCongs
                .Where(c => c.NgayChamCong.Date == today)
                .OrderByDescending(c => c.GioVao)
                .Select(c => new {
                    c.MaNV,
                    c.GioVao,
                    c.GioRa,
                    c.TrangThai
                })
                .ToListAsync();

            return Ok(records);
        }
    }
}