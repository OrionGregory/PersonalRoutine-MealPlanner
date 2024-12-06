using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Microsoft.AspNetCore.Identity;

namespace Assignment3.Controllers
{
    [Authorize]
    public class NutritionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NutritionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Nutrition
// GET: Nutrition
[HttpGet]
public async Task<IActionResult> Index()
{
    var userId = _userManager.GetUserId(User);
    var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == userId);
    if (person == null)
    {
        return RedirectToAction("Create", "Person");
    }

    var nutritionList = await _context.Nutrition
        .Include(n => n.Person)
        .Where(n => n.Person.UserId == userId)
        .ToListAsync();
    return View(nutritionList);
}

        // GET: Nutrition/Details/5
[HttpGet]
public async Task<IActionResult> Details(int id)
{
    var nutrition = await _context.Nutrition.Include(n => n.Person)
        .FirstOrDefaultAsync(n => n.Id == id);
    if (nutrition == null) return NotFound();
    return View(nutrition);
}

// GET: Nutrition/Create
[HttpGet]
public async Task<IActionResult> Create()
{
    var userId = _userManager.GetUserId(User);
    var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == userId);
    
    if (person == null)
    {
        return RedirectToAction("Create", "Person");
    }

    var nutrition = new Nutrition { PersonId = person.Id };
    return View(nutrition);
}

    // POST: Nutrition/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Nutrition nutrition)
    {
        var userId = _userManager.GetUserId(User);
        var person = await _context.People
            .FirstOrDefaultAsync(p => p.Id == nutrition.PersonId && p.UserId == userId);

        if (person == null)
        {
            ModelState.AddModelError("", "Invalid PersonId or unauthorized access.");
            return View(nutrition);
        }

        if (ModelState.IsValid)
        {
            _context.Nutrition.Add(nutrition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(nutrition);
    }

        // GET: Nutrition/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition == null) return NotFound();
            return View(nutrition);
        }

        // POST: Nutrition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Nutrition nutrition)
        {
            if (ModelState.IsValid)
            {
                _context.Update(nutrition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nutrition);
        }

        // GET: Nutrition/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition == null) return NotFound();
            return View(nutrition);
        }

        // POST: Nutrition/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition != null)
            {
                _context.Nutrition.Remove(nutrition);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
