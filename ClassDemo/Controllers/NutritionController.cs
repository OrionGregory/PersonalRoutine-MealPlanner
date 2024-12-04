using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

namespace Assignment3.Controllers
{
    [Authorize]
    public class NutritionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NutritionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Nutrition
        public async Task<IActionResult> Index()
        {
            var nutritionList = await _context.Nutrition.Include(n => n.Person).ToListAsync();
            return View(nutritionList);
        }

        // GET: Nutrition/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var nutrition = await _context.Nutrition.Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nutrition == null) return NotFound();
            return View(nutrition);
        }

        // GET: Nutrition/Create
        public IActionResult Create(int personId)
        {
            var nutrition = new Nutrition { PersonId = personId };
            return View(nutrition);
        }

        // POST: Nutrition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nutrition nutrition)
        {
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
