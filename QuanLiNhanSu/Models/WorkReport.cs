using System.ComponentModel.DataAnnotations;
namespace QuanLiNhanSu.Models
{
    public class WorkReport
    {
        public int Id { get; set; }
        public string Username { get; set; } // Ai là người báo cáo
        [Required] public string TenKhachHang { get; set; }
        public string NoiDungCongViec { get; set; }
        public int SoGioLam { get; set; }
        public DateTime NgayBaoCao { get; set; } = DateTime.Now;
    }
}