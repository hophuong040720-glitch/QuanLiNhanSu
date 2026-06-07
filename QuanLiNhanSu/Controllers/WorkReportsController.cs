using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLiNhanSu.Models;

[Authorize]
public class WorkReportsController : Controller
{
    private readonly AppDbContext _context;
    public WorkReportsController(AppDbContext context) { _context = context; }

    public IActionResult Index()
    {
        var reports = _context.WorkReports.Where(r => r.Username == User.Identity.Name || User.IsInRole("Admin")).ToList();
        return View(reports);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(WorkReport report)
    {
        report.Username = User.Identity.Name;
        ModelState.Remove("Username"); // Bỏ qua validation do trường này tự gán

        if (ModelState.IsValid)
        {
            _context.WorkReports.Add(report);
            _context.SaveChanges();
            TempData["Success"] = "Đã gửi báo cáo công việc!";
            return RedirectToAction(nameof(Index));
        }
        return View(report);
    }
}