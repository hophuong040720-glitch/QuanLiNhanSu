using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using QuanLiNhanSu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using QuanLiNhanSu.Hubs;
using Microsoft.Extensions.Configuration;

namespace QuanLiNhanSu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FingerprintController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.AuditService _audit;
        private readonly IHubContext<ChamCongHub> _hubContext;
        private readonly IConfiguration _configuration;

        public FingerprintController(AppDbContext context, Services.AuditService audit, IHubContext<ChamCongHub> hubContext, IConfiguration configuration) 
        { 
            _context = context; 
            _audit = audit;
            _hubContext = hubContext;
            _configuration = configuration;
        }

        public class FingerprintData
        {
            public string DeviceId { get; set; }
            public string MaNV { get; set; }
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ReceiveScanData([FromBody] FingerprintData data)
        {
            // 1. KIỂM TRA BẢO MẬT API KEY (Tránh việc dùng Postman gọi bừa bãi)
            var expectedApiKey = _configuration.GetValue<string>("FingerprintApiKey");
            if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey) || extractedApiKey != expectedApiKey)
            {
                return Unauthorized(new { status = "error", message = "API Key không hợp lệ hoặc bị thiếu!" });
            }

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

                // PUSH THÔNG BÁO SIGNALR LÊN TRÌNH DUYỆT
                var msg = "Ghi nhận giờ vào thành công!";
                await _hubContext.Clients.All.SendAsync("ReceiveFingerprintScan", data.MaNV, "CheckIn", msg);

                return Ok(new { status = "success", action = "CheckIn", message = msg });
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

                // PUSH THÔNG BÁO SIGNALR LÊN TRÌNH DUYỆT
                var msg = $"Ghi nhận giờ ra thành công! Tổng giờ: {attendanceToday.SoGioLam} tiếng.";
                await _hubContext.Clients.All.SendAsync("ReceiveFingerprintScan", data.MaNV, "CheckOut", msg);

                return Ok(new { status = "success", action = "CheckOut", message = msg });
            }

            var warningMsg = "Nhân viên này đã hoàn thành chấm công ra vào trong ngày!";
            await _hubContext.Clients.All.SendAsync("ReceiveFingerprintScan", data.MaNV, "Warning", warningMsg);
            return Ok(new { status = "warning", message = warningMsg });
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