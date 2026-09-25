
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

        // GET: ACADEMICPROGRAMS
        public async Task<IActionResult> Index()
        {
            return View(await _context.AcademicPrograms.ToListAsync());
        }

        // GET: ACADEMICPROGRAMS/Details/5
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

        // GET: ACADEMICPROGRAMS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ACADEMICPROGRAMS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: ACADEMICPROGRAMS/Edit/5
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

        // POST: ACADEMICPROGRAMS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: ACADEMICPROGRAMS/Delete/5
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

        // POST: ACADEMICPROGRAMS/Delete/5
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
