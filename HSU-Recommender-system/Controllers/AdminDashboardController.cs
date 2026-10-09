using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HSU_Recommender_system.Data;

namespace HSU_Recommender_system.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalStudents = await _context.StudentProfiles.CountAsync();
            ViewBag.TotalUniversities = await _context.Universities.CountAsync();
            ViewBag.TotalPrograms = await _context.AcademicPrograms.CountAsync();
            ViewBag.TotalShortlists = await _context.ShortlistedPrograms.CountAsync();

            var popularPrograms = await _context.ShortlistedPrograms
                .Include(s => s.AcademicProgram)
                .ThenInclude(ap => ap.University)
                .GroupBy(s => s.AcademicProgram.University.Name)
                .Select(g => new
                {
                    UniversityName = g.Key,
                    SaveCount = g.Count()
                })
                .OrderByDescending(x => x.SaveCount)
                .Take(5)
                .ToListAsync();

            ViewBag.ChartLabels = popularPrograms.Select(p => p.UniversityName).ToArray();
            ViewBag.ChartData = popularPrograms.Select(p => p.SaveCount).ToArray();

            var statusBreakdown = await _context.ShortlistedPrograms
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusBreakdown.Select(s => s.Status).ToArray();
            ViewBag.StatusData = statusBreakdown.Select(s => s.Count).ToArray();

            return View();
        }
    }
}