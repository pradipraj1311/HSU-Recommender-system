
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HSU_Recommender_system.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AcademicProgramsController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AcademicProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.AcademicPrograms.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicprogram = await _context.AcademicPrograms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (academicprogram == null)
            {
                return NotFound();
            }

            return View(academicprogram);
        }

        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UniversityId,University,DegreeName,Department,TotalTuitionFee,MinCGPA,MinIELTS,HistoricalRAChance,HistoricalTAChance,ResearchKeywords,Deadlines")] AcademicProgram academicprogram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(academicprogram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(academicprogram);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicprogram = await _context.AcademicPrograms.FindAsync(id);
            if (academicprogram == null)
            {
                return NotFound();
            }
            return View(academicprogram);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,UniversityId,University,DegreeName,Department,TotalTuitionFee,MinCGPA,MinIELTS,HistoricalRAChance,HistoricalTAChance,ResearchKeywords,Deadlines")] AcademicProgram academicprogram)
        {
            if (id != academicprogram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(academicprogram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicProgramExists(academicprogram.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(academicprogram);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var academicprogram = await _context.AcademicPrograms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (academicprogram == null)
            {
                return NotFound();
            }

            return View(academicprogram);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var academicprogram = await _context.AcademicPrograms.FindAsync(id);
            if (academicprogram != null)
            {
                _context.AcademicPrograms.Remove(academicprogram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AcademicProgramExists(int? id)
        {
            return _context.AcademicPrograms.Any(e => e.Id == id);
        }

    }
}
