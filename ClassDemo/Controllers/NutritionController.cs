using Assignment3.Models;
using Assignment3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Assignment3.Controllers
{
    [Authorize]
    public class NutritionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AIAnalysisService _aiAnalysisService;

        public NutritionController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AIAnalysisService aiAnalysisService)
        {
            _context = context;
            _userManager = userManager;
            _aiAnalysisService = aiAnalysisService;
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
            var nutrition = await _context.Nutrition
                .Include(n => n.Person)
                .Include(n => n.Meals)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nutrition == null)
            {
                return NotFound();
            }

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
            if (nutrition == null)
            {
                return NotFound();
            }
            return View(nutrition);
        }

        // POST: Nutrition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Nutrition nutrition)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nutrition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NutritionExists(nutrition.Id))
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
            return View(nutrition);
        }

        // GET: Nutrition/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var nutrition = await _context.Nutrition.FindAsync(id);
            if (nutrition == null)
            {
                return NotFound();
            }
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

        // GET: Nutrition/GenerateNutritionPlan/{id?}
// GET: Nutrition/GenerateNutritionPlan/{id?}
[HttpGet("Nutrition/GenerateNutritionPlan/{id?}")]
public async Task<IActionResult> GenerateNutritionPlan(int? id)
{
    if (!id.HasValue)
    {
        return BadRequest("Nutrition plan ID is required.");
    }

    var userId = _userManager.GetUserId(User);
    var nutrition = await _context.Nutrition
        .Include(n => n.Person)
            .ThenInclude(p => p.Routines)
                .ThenInclude(r => r.Exercises)
        .Include(n => n.Meals)
        .FirstOrDefaultAsync(n => n.Id == id.Value);

    if (nutrition == null || nutrition.Person.UserId != userId)
    {
        return NotFound();
    }

    var person = nutrition.Person;

    // Calculate nutrition values
    var bmr = NutritionCalculator.CalculateBMR(person);
    var averageRoutineCaloriesBurned = NutritionCalculator.CalculateAverageRoutineCaloriesBurned(person);
    var dailyCalorieAdjustment = NutritionCalculator.CalculateDailyCalorieAdjustment(person);
    var totalDailyCalories = NutritionCalculator.GetTotalDailyCalories(person);
    var (proteinPct, carbsPct, fatPct) = NutritionCalculator.CalculateMacroPercentages(person);

    // Generate meals using AIAnalysisService
    var generatedMeals = await _aiAnalysisService.GenerateMealsFromAI(totalDailyCalories, proteinPct, carbsPct, fatPct);

    // Clear existing meals
    _context.Meals.RemoveRange(nutrition.Meals);

    // Assign NutritionId to each generated meal
    foreach (var meal in generatedMeals)
    {
        meal.NutritionId = nutrition.Id;
    }

    // Add generated meals
    nutrition.Meals = generatedMeals;

    // Update nutrition plan
    nutrition.BMR = bmr;
    nutrition.RoutineCaloriesBurned = averageRoutineCaloriesBurned;
    nutrition.CalorieSurplusOrDeficit = dailyCalorieAdjustment;
    nutrition.TotalDailyCalories = totalDailyCalories;
    nutrition.ProteinPercentage = proteinPct;
    nutrition.CarbPercentage = carbsPct;
    nutrition.FatPercentage = fatPct;

    _context.Nutrition.Update(nutrition);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Details), new { id = nutrition.Id });
}
        private bool NutritionExists(int id)
        {
            return _context.Nutrition.Any(e => e.Id == id);
        }
    }
}