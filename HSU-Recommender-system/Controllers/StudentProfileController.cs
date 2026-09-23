using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;

namespace HSU_Recommender_system.Controllers
{
    [Authorize]
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

            await PopulateDynamicDropdownsAsync();

            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            return View(profile ?? new StudentProfile());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(StudentProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Authentication required.");

            if (ModelState.IsValid)
            {
                var existingProfile = await _context.StudentProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (existingProfile == null)
                {
                    model.UserId = user.Id;
                    _context.Add(model);
                }
                else
                {
                    existingProfile.CGPA = model.CGPA;
                    existingProfile.EnglishProficiencyScore = model.EnglishProficiencyScore;
                    existingProfile.MaximumBudget = model.MaximumBudget;
                    existingProfile.NumberOfProjects = model.NumberOfProjects;
                    existingProfile.NumberOfResearchPapers = model.NumberOfResearchPapers;
                    existingProfile.WorkExperienceMonths = model.WorkExperienceMonths;
                    existingProfile.TargetDegree = model.TargetDegree;
                    existingProfile.PreferredCountry = model.PreferredCountry;
                    
                    _context.Update(existingProfile);
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profile parameters updated successfully!";
                return RedirectToAction(nameof(Manage));
            }
            
           
            await PopulateDynamicDropdownsAsync();
            return View(model);
        }
    }
}
