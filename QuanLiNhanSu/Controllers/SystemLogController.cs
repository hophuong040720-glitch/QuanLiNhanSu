using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLiNhanSu.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLiNhanSu.Controllers
{
    [Authorize(Roles = "Admin")] // Cực kỳ quan trọng: Chỉ Admin tối cao mới được xem Log
    public class SystemLogController : Controller
    {
        private readonly AppDbContext _context;
        public SystemLogController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.SystemLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
            return View(logs);
        }
    }
}