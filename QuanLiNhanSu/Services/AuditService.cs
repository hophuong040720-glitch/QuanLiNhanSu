using QuanLiNhanSu.Models;

namespace QuanLiNhanSu.Services
{
    /// <summary>
    /// Service ghi nhật ký hành động (Audit Trail) vào bảng SystemLogs.
    /// Inject vào bất kỳ Controller nào cần ghi vết hoạt động.
    /// </summary>
    public class AuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ghi một bản ghi audit vào SystemLogs và lưu ngay vào DB.
        /// </summary>
        /// <param name="username">Tên tài khoản thực hiện hành động</param>
        /// <param name="action">Hành động (Thêm NV, Xóa NV, Đăng nhập, Duyệt lương...)</param>
        /// <param name="target">Đối tượng bị tác động (Employees, Users, BangLuong...)</param>
        /// <param name="details">Chi tiết cụ thể (Ví dụ: "Thêm nhân viên NV006 - Ngô Văn A")</param>
        public async Task LogAsync(string username, string action, string target, string details)
        {
            try
            {
                _context.SystemLogs.Add(new SystemLog
                {
                    Username = username ?? "Unknown",
                    Action = action,
                    Target = target,
                    Timestamp = DateTime.Now,
                    Details = details
                });
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Không để lỗi ghi log làm crash toàn bộ ứng dụng
            }
        }

        /// <summary>
        /// Overload đồng bộ (synchronous) cho các context không dùng async.
        /// </summary>
        public void Log(string username, string action, string target, string details)
        {
            try
            {
                _context.SystemLogs.Add(new SystemLog
                {
                    Username = username ?? "Unknown",
                    Action = action,
                    Target = target,
                    Timestamp = DateTime.Now,
                    Details = details
                });
                _context.SaveChanges();
            }
            catch
            {
                // Không để lỗi ghi log làm crash toàn bộ ứng dụng
            }
        }
    }
}
