using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;

namespace HSU_Recommender_system.Controllers
{
    [Authorize(Roles = "Student")]
    public class ShortlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShortlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            var shortlist = await _context.ShortlistedPrograms
                .Include(s => s.AcademicProgram)
                .ThenInclude(ap => ap.University)
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.AddedOn)
                .ToListAsync();

            return View(shortlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int programId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool exists = await _context.ShortlistedPrograms
                .AnyAsync(s => s.UserId == user.Id && s.AcademicProgramId == programId);

            if (!exists)
            {
                var item = new ShortlistedProgram
                {
                    UserId = user.Id,
                    AcademicProgramId = programId,
                    Status = "Saved"
                };
                _context.ShortlistedPrograms.Add(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Program successfully saved to your Shortlist!";
            }
            else
            {
                TempData["ErrorMessage"] = "This program is already in your Shortlist.";
            }

            return RedirectToAction("Index", "Recommendations");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var item = await _context.ShortlistedPrograms.FindAsync(id);
            if (item != null)
            {
                item.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Application status updated!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.ShortlistedPrograms.FindAsync(id);
            if (item != null)
            {
                _context.ShortlistedPrograms.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Program removed from your shortlist.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}