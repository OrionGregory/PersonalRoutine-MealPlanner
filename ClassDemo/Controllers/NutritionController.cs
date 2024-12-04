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

        public async Task<IActionResult> Index()
        {
            // Get all nutrition records along with associated person data
            var nutritionList = await _context.Nutrition.Include(n => n.Person).ToListAsync();
            return View(nutritionList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var nutrition = await _context.Nutrition.Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id);
            if (nutrition == null) return NotFound();
            return View(nutrition);
        }

        public IActionResult Create(int personId)
        {
            var nutrition = new Nutrition { PersonId = personId };
            return View(nutrition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nutrition nutrition)
        {
            if (ModelState.IsValid)
            {
                _context.Nutrition.Add(nutrition);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = nutrition.Id });
            }
            return View(nutrition);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition == null) return NotFound();
            return View(nutrition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Nutrition nutrition)
        {
            if (ModelState.IsValid)
            {
                _context.Update(nutrition);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = nutrition.Id });
            }
            return View(nutrition);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition == null) return NotFound();
            _context.Nutrition.Remove(nutrition);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Person", new { id = nutrition.PersonId });
        }
    }
}
