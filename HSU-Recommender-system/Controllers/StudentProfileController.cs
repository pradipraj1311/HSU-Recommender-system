using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;

namespace HSU_Recommender_system.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task PopulateDynamicDropdownsAsync()
        {
            ViewBag.Countries = new SelectList(await _context.Universities.Select(u => u.Country).Distinct().OrderBy(c => c).ToListAsync());
            ViewBag.Degrees = new SelectList(await _context.AcademicPrograms.Select(p => p.DegreeName).Distinct().OrderBy(d => d).ToListAsync());

            // DYNAMICALLY EXTRACT ALL RESEARCH KEYWORDS UPLOADED BY ADMIN
            var allKeywords = await _context.AcademicPrograms
                .Where(p => !string.IsNullOrWhiteSpace(p.ResearchKeywords))
                .Select(p => p.ResearchKeywords)
                .ToListAsync();

            var distinctKeywords = allKeywords
                .Where(k => k != null)
                .SelectMany(k => k!.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(k => k.Trim())
                .Distinct()
                .OrderBy(k => k)
                .ToList();

            ViewBag.ResearchKeywords = new SelectList(distinctKeywords);
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            var existingProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id) ?? new StudentProfile();

            await PopulateDynamicDropdownsAsync();
            return View(existingProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(StudentProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (existingProfile == null)
                {
                    model.UserId = user.Id;
                    _context.StudentProfiles.Add(model);
                }
                else
                {
                    existingProfile.CGPA = model.CGPA;
                    existingProfile.EnglishProficiencyScore = model.EnglishProficiencyScore;
                    existingProfile.GREQuantScore = model.GREQuantScore;
                    existingProfile.ResearchInterests = model.ResearchInterests;
                    existingProfile.NumberOfResearchPapers = model.NumberOfResearchPapers;
                    existingProfile.WorkExperienceMonths = model.WorkExperienceMonths;
                    existingProfile.TargetDegree = model.TargetDegree;
                    existingProfile.PreferredCountry = model.PreferredCountry;

                    // We intentionally ignore Budget and MajorProjects here as requested
                    _context.StudentProfiles.Update(existingProfile);
                }

                await _context.SaveChangesAsync();

                // REDIRECT DIRECTLY TO RECOMMENDATIONS INSTEAD OF RELOADING THE FORM
                return RedirectToAction("Index", "Recommendations");
            }

            await PopulateDynamicDropdownsAsync();
            return View(model);
        }
    }
}