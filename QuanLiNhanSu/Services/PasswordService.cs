using BC = BCrypt.Net.BCrypt;

namespace QuanLiNhanSu.Services
{
    /// <summary>
    /// Service xử lý mã hóa và xác minh mật khẩu bằng BCrypt.
    /// BCrypt tự động tạo salt ngẫu nhiên cho mỗi lần hash, đảm bảo an toàn tuyệt đối.
    /// </summary>
    public class PasswordService
    {
        /// <summary>
        /// Băm mật khẩu plain text thành chuỗi BCrypt hash an toàn.
        /// WorkFactor = 12 (cân bằng tốt giữa bảo mật và hiệu năng).
        /// </summary>
        public string HashPassword(string plainText)
        {
            return BC.HashPassword(plainText, workFactor: 12);
        }

        /// <summary>
        /// Xác minh mật khẩu plain text với hash đã lưu trong DB.
        /// Hỗ trợ cả 2 trường hợp:
        ///   1. Mật khẩu cũ (plain text) - cho phép auto-migrate mượt mà
        ///   2. Mật khẩu mới (đã hash bằng BCrypt)
        /// </summary>
        public bool VerifyPassword(string plainText, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            // Kiểm tra xem chuỗi stored có phải là BCrypt hash không
            // BCrypt hash luôn bắt đầu bằng "$2a$", "$2b$" hoặc "$2y$"
            if (storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$"))
            {
                // Mật khẩu đã được hash → dùng BCrypt.Verify
                try
                {
                    return BC.Verify(plainText, storedHash);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Mật khẩu cũ lưu plain text → so sánh trực tiếp
                // (Account cũ sẽ được auto-hash lại ở AccountController sau khi login thành công)
                return plainText == storedHash;
            }
        }

        /// <summary>
        /// Kiểm tra xem mật khẩu đã được hash chưa.
        /// Dùng để xác định có cần auto-migrate hay không.
        /// </summary>
        public bool IsHashed(string password)
        {
            return !string.IsNullOrEmpty(password) &&
                   (password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$"));
        }
    }
}
