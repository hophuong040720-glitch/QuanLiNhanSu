using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLiNhanSu.Models;

[Authorize(Roles = "Admin,Employee")]
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
    public IActionResult Create(WorkReport report)
    {
        report.Username = User.Identity.Name;
        _context.WorkReports.Add(report);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}