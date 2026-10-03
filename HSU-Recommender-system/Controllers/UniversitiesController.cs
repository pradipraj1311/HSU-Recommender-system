
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace HSU_Recommender_system.Controllers{


[Authorize(Roles = "Admin")]

public class UniversitiesController : Controller
{

    private readonly ApplicationDbContext _context;

    public UniversitiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: UNIVERSITYS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Universities.ToListAsync());
    }

    // GET: UNIVERSITYS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var university = await _context.Universities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (university == null)
        {
            return NotFound();
        }

        return View(university);
    }

    // GET: UNIVERSITYS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: UNIVERSITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Country,StateOrCity,EstimatedLivingCost,Ranking,Programs")] University university)
    {
        if (ModelState.IsValid)
        {
            _context.Add(university);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(university);
    }

    // GET: UNIVERSITYS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var university = await _context.Universities.FindAsync(id);
        if (university == null)
        {
            return NotFound();
        }
        return View(university);
    }

    // POST: UNIVERSITYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Country,StateOrCity,EstimatedLivingCost,Ranking,Programs")] University university)
    {
        if (id != university.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(university);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UniversityExists(university.Id))
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
        return View(university);
    }

    // GET: UNIVERSITYS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var university = await _context.Universities
            .FirstOrDefaultAsync(m => m.Id == id);
        if (university == null)
        {
            return NotFound();
        }

        return View(university);
    }

    // POST: UNIVERSITYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var university = await _context.Universities.FindAsync(id);
        if (university != null)
        {
            _context.Universities.Remove(university);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid CSV file.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreBlankLines = true
                };

                using var csv = new CsvReader(stream, csvConfig);

                // ૧. CSV માંથી બધો કમ્બાઇન્ડ ડેટા DTO માં લોડ કરો
                var records = csv.GetRecords<ImportDTO>().ToList();

                if (records.Any())
                {
                    int uniCount = 0;
                    int progCount = 0;

                    // ૨. ડુપ્લિકેટ ના થાય તે માટે યુનિવર્સિટીના નામ પ્રમાણે ગ્રુપ બનાવો
                    var uniqueUniversities = records.GroupBy(r => r.UniversityName).ToList();

                    foreach (var uniGroup in uniqueUniversities)
                    {
                        var firstRow = uniGroup.First();

                        // ચેક કરો કે ડેટાબેઝમાં યુનિવર્સિટી પહેલેથી છે કે નહીં?
                        var university = await _context.Universities
                            .FirstOrDefaultAsync(u => u.Name == firstRow.UniversityName);

                        if (university == null)
                        {
                            // નવી યુનિવર્સિટી બનાવો
                            university = new University
                            {
                                Name = firstRow.UniversityName,
                                Country = firstRow.Country,
                                StateOrCity = firstRow.StateOrCity,
                                EstimatedLivingCost = firstRow.EstimatedLivingCost,
                                CarnegieClassification = firstRow.CarnegieClassification,
                                StudentFacultyRatio = firstRow.StudentFacultyRatio,
                                Ranking = firstRow.Ranking
                            };
                            _context.Universities.Add(university);
                            await _context.SaveChangesAsync(); // આનાથી નવી યુનિવર્સિટીનો Id તરત જનરેટ થશે
                            uniCount++;
                        }

                        // ૩. હવે આ યુનિવર્સિટીના ગ્રુપમાં રહેલા બધા પ્રોગ્રામ્સને ડેટાબેઝમાં નાખો
                        foreach (var progRow in uniGroup)
                        {
                            // ડુપ્લિકેટ પ્રોગ્રામ ચેક
                            var programExists = await _context.AcademicPrograms
                                .AnyAsync(p => p.DegreeName == progRow.DegreeName && p.UniversityId == university.Id);

                            if (!programExists && !string.IsNullOrEmpty(progRow.DegreeName))
                            {
                                var program = new AcademicProgram
                                {
                                    UniversityId = university.Id, // અહીં લિંક થયું!
                                    DegreeName = progRow.DegreeName,
                                    Department = progRow.Department,
                                    TotalTuitionFee = progRow.TotalTuitionFee,
                                    MinCGPA = progRow.MinCGPA,
                                    MinIELTS = progRow.MinIELTS,
                                    TargetGREQuant = progRow.TargetGREQuant,
                                    IsGRERequired = progRow.IsGRERequired,
                                    HistoricalRAChance = progRow.HistoricalRAChance,
                                    HistoricalTAChance = progRow.HistoricalTAChance,
                                    ResearchKeywords = progRow.ResearchKeywords,
                                    OpenAlexInstitutionId = progRow.OpenAlexInstitutionId
                                };
                                _context.AcademicPrograms.Add(program);
                                progCount++;
                            }
                        }
                    }

                    await _context.SaveChangesAsync(); // ફાઇનલ સેવ
                    TempData["SuccessMessage"] = $"Successfully imported {uniCount} new Universities and {progCount} new Academic Programs!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error importing data: Check your CSV headers. Details: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UniversityExists(int? id)
    {
        return _context.Universities.Any(e => e.Id == id);
    }
}
    }
