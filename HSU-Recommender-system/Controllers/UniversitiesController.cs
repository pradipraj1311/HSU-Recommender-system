
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

    public async Task<IActionResult> Index()    
    {
        return View(await _context.Universities.ToListAsync());
    }

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

    public IActionResult Create()
    {
        return View();
    }

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
    public async Task<IActionResult> UploadUniversitiesCsv(IFormFile file)
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

        var records = csv.GetRecords<UniversityCsvDTO>().ToList();
        var addedCount = 0;

        foreach (var record in records)
        {
            if (!await _context.Universities.AnyAsync(u => u.Name == record.Name))
            {
                var university = new University
                {
                    Name = record.Name,
                    Country = record.Country,
                    StateOrCity = record.StateOrCity,
                    EstimatedLivingCost = record.EstimatedLivingCost,
                    Ranking = record.Ranking,
                    CarnegieClassification = record.CarnegieClassification,
                    StudentFacultyRatio = record.StudentFacultyRatio
                };
                _context.Universities.Add(university);
                addedCount++;
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Successfully imported {addedCount} new universities.";
        return RedirectToAction(nameof(Index));
    }
    private bool UniversityExists(int? id)
    {
        return _context.Universities.Any(e => e.Id == id);
    }
}
    }
