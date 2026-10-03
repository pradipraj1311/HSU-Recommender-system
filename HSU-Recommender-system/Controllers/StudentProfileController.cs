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

            var countries = await _context.Universities
                .Select(u => u.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Countries = new SelectList(countries);


            var degrees = await _context.AcademicPrograms
                .Select(p => p.DegreeName)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            ViewBag.Degrees = new SelectList(degrees);
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            // FETCH EXISTING DATA: If the user has a profile, load it. Otherwise, create a blank one.
            var existingProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (existingProfile == null)
            {
                existingProfile = new StudentProfile();
            }

            // Populate dropdowns
            ViewBag.Degrees = new SelectList(await _context.AcademicPrograms.Select(p => p.DegreeName).Distinct().ToListAsync());
            ViewBag.Countries = new SelectList(await _context.Universities.Select(u => u.Country).Distinct().ToListAsync());

            return View(existingProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(StudentProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            // STRICT VALIDATION BYPASS: Ignore navigation properties and primary keys
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
                    existingProfile.NumberOfProjects = model.NumberOfProjects;
                    existingProfile.NumberOfResearchPapers = model.NumberOfResearchPapers;
                    existingProfile.MaximumBudget = model.MaximumBudget;
                    existingProfile.WorkExperienceMonths = model.WorkExperienceMonths;
                    existingProfile.TargetDegree = model.TargetDegree;
                    existingProfile.PreferredCountry = model.PreferredCountry;

                    _context.StudentProfiles.Update(existingProfile);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile saved successfully! Click 'My Matches' to see your university recommendations.";
                return RedirectToAction(nameof(Manage));
            }

            // If it still fails, reload dropdowns so the UI doesn't crash
            ViewBag.Degrees = new SelectList(await _context.AcademicPrograms.Select(p => p.DegreeName).Distinct().ToListAsync());
            ViewBag.Countries = new SelectList(await _context.Universities.Select(u => u.Country).Distinct().ToListAsync());

            return View(model);
        }
    }
    }
