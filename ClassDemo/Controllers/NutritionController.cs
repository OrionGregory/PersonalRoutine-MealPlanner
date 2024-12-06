using Assignment3.Models;
using Assignment3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
                // Ensure the PersonId exists
                var person = await _context.People.FindAsync(nutrition.PersonId);
                if (person == null)
                {
                    ModelState.AddModelError("", "Invalid PersonId.");
                    return View(nutrition);
                }

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

        // GET: Nutrition/GenerateNutritionPlan
        public async Task<IActionResult> GenerateNutritionPlan()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                .Include(p => p.Routines)
                    .ThenInclude(r => r.Exercises)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return RedirectToAction("Create", "Person");
            }

            // Calculate nutrition values
            var bmr = NutritionCalculator.CalculateBMR(person);
            var averageRoutineCaloriesBurned = NutritionCalculator.CalculateAverageRoutineCaloriesBurned(person);
            var dailyCalorieAdjustment = NutritionCalculator.CalculateDailyCalorieAdjustment(person);
            var totalDailyCalories = NutritionCalculator.GetTotalDailyCalories(person);
            var (proteinPct, carbsPct, fatPct) = NutritionCalculator.CalculateMacroPercentages(person);

            // Check if a nutrition plan already exists
            var existingNutrition = await _context.Nutrition.FirstOrDefaultAsync(n => n.PersonId == person.Id);

            if (existingNutrition != null)
            {
                // Update existing nutrition plan
                existingNutrition.BMR = bmr;
                existingNutrition.CalorieSurplusOrDeficit = dailyCalorieAdjustment;
                existingNutrition.RoutineCaloriesBurned = averageRoutineCaloriesBurned;
                existingNutrition.TotalDailyCalories = totalDailyCalories;
                existingNutrition.ProteinPercentage = proteinPct;
                existingNutrition.CarbPercentage = carbsPct;
                existingNutrition.FatPercentage = fatPct;

                _context.Nutrition.Update(existingNutrition);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = existingNutrition.Id });
            }
            else
            {
                // Create new nutrition plan
                var nutrition = new Nutrition
                {
                    PersonId = person.Id,
                    BMR = bmr,
                    CalorieSurplusOrDeficit = dailyCalorieAdjustment,
                    RoutineCaloriesBurned = averageRoutineCaloriesBurned,
                    TotalDailyCalories = totalDailyCalories,
                    ProteinPercentage = proteinPct,
                    CarbPercentage = carbsPct,
                    FatPercentage = fatPct
                };

                _context.Nutrition.Add(nutrition);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = nutrition.Id });
            }
        }
    }
}