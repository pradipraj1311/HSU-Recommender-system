
using HSU_Recommender_system.Data;
using HSU_Recommender_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    private bool UniversityExists(int? id)
    {
        return _context.Universities.Any(e => e.Id == id);
    }
}
    }
