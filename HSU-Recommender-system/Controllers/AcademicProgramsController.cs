
using CsvHelper;
using CsvHelper.Configuration;
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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

    
[HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProgramsCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid CSV file.";
            return RedirectToAction(nameof(Index));
        }

        using var stream = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var records = csv.GetRecords<ProgramCsvDTO>().ToList();
        var addedCount = 0;
        var notFoundCount = 0;

        foreach (var record in records)
        {
            var university = await _context.Universities
                .FirstOrDefaultAsync(u => u.Name.ToLower() == record.UniversityName.ToLower());

            if (university != null)
            {
                var program = new AcademicProgram
                {
                    UniversityId = university.Id,
                    DegreeName = record.DegreeName,
                    Department = record.Department,
                    TotalTuitionFee = record.TotalTuitionFee,
                    MinCGPA = record.MinCGPA,
                    MinIELTS = record.MinIELTS,
                    HistoricalRAChance = record.HistoricalRAChance,
                    HistoricalTAChance = record.HistoricalTAChance,
                    TargetGREQuant = record.TargetGREQuant,
                    IsGRERequired = record.IsGRERequired,
                    ResearchKeywords = record.ResearchKeywords,
                    OpenAlexInstitutionId = record.OpenAlexInstitutionId
                };

                _context.AcademicPrograms.Add(program);
                addedCount++;
            }
            else
            {
                notFoundCount++;
            }
        }

        await _context.SaveChangesAsync();

        if (notFoundCount > 0)
        {
            TempData["WarningMessage"] = $"Imported {addedCount} programs. Skipped {notFoundCount} programs because the University Name didn't match any existing records.";
        }
        else
        {
            TempData["SuccessMessage"] = $"Successfully imported {addedCount} programs.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AcademicProgramExists(int? id)
        {
            return _context.AcademicPrograms.Any(e => e.Id == id);
        }

    }
}
