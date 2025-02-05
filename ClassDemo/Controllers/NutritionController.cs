﻿using Assignment3.Models;
using Assignment3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace Assignment3.Controllers
{
    [Authorize]
    public class NutritionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AIAnalysisService _aiAnalysisService;
        private readonly ILogger<NutritionController> _logger;

        public NutritionController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AIAnalysisService aiAnalysisService, ILogger<NutritionController> logger)
        {
            _context = context;
            _userManager = userManager;
            _aiAnalysisService = aiAnalysisService;
            _logger = logger;
        }

        // GET: Nutrition
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var nutritionList = await _context.Nutrition
                .Include(n => n.Person)
                .Where(n => isAdmin || n.Person.UserId == userId)
                .ToListAsync();
            return View(nutritionList);
        }

        // GET: Nutrition/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var nutrition = await _context.Nutrition
                .Include(n => n.Person)
                .Include(n => n.Meals)
                .FirstOrDefaultAsync(n => n.Id == id && (isAdmin || n.Person.UserId == userId));

            if (nutrition == null)
            {
                return NotFound();
            }

            return View(nutrition);
        }

        // GET: Nutrition/Create
        public IActionResult Create(int personId)
        {
            var userId = _userManager.GetUserId(User);
            var user = _context.People.FirstOrDefault(p => p.Id == personId && p.UserId == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var nutrition = new Nutrition { PersonId = personId };
            return View(nutrition);
        }

        // POST: Nutrition/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nutrition nutrition)
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == nutrition.PersonId && p.UserId == userId);
            if (person == null)
            {
                return Unauthorized();
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
        [HttpGet("Nutrition/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var nutrition = await _context.Nutrition
                .Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id && (isAdmin || n.Person.UserId == userId));

            if (nutrition == null)
            {
                _logger.LogWarning("Nutrition plan with ID {NutritionId} not found for user {UserId}", id, userId);
                return NotFound();
            }
            return View(nutrition);
        }

        // POST: Nutrition/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Nutrition nutrition)
        {
            if (id != nutrition.Id)
            {
                _logger.LogWarning("Nutrition ID mismatch: {NutritionId} does not match {ProvidedId}", nutrition.Id, id);
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var existingNutrition = await _context.Nutrition
                .Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id && (isAdmin || n.Person.UserId == userId));

            if (existingNutrition == null)
            {
                _logger.LogWarning("Nutrition plan with ID {NutritionId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the properties of the existing nutrition plan
                    existingNutrition.BMR = nutrition.BMR;
                    existingNutrition.CalorieSurplusOrDeficit = nutrition.CalorieSurplusOrDeficit;
                    existingNutrition.CarbPercentage = nutrition.CarbPercentage;
                    existingNutrition.FatPercentage = nutrition.FatPercentage;
                    existingNutrition.ProteinPercentage = nutrition.ProteinPercentage;
                    existingNutrition.RoutineCaloriesBurned = nutrition.RoutineCaloriesBurned;
                    existingNutrition.TotalDailyCalories = nutrition.TotalDailyCalories;

                    _context.Update(existingNutrition);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Nutrition plan with ID {NutritionId} updated successfully for user {UserId}", id, userId);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!NutritionExists(nutrition.Id))
                    {
                        _logger.LogWarning("Nutrition plan with ID {NutritionId} no longer exists", nutrition.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error occurred while updating nutrition plan with ID {NutritionId} for user {UserId}", id, userId);
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Log model state errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Model state error: {ErrorMessage}", error.ErrorMessage);
            }

            _logger.LogWarning("Model state is invalid for nutrition plan with ID {NutritionId} for user {UserId}", id, userId);
            return View(nutrition);
        }

        // GET: Nutrition/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var nutrition = await _context.Nutrition
                .Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id && (isAdmin || n.Person.UserId == userId));

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
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var nutrition = await _context.Nutrition
                .Include(n => n.Person)
                .FirstOrDefaultAsync(n => n.Id == id && (isAdmin || n.Person.UserId == userId));

            if (nutrition == null)
            {
                return NotFound();
            }

            _context.Nutrition.Remove(nutrition);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Nutrition/GenerateNutritionPlan/{id?}
        [HttpGet("Nutrition/GenerateNutritionPlan")]
        public async Task<IActionResult> GenerateNutritionPlan()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var person = await _context.People
                .Include(p => p.Routines)
                    .ThenInclude(r => r.Exercises)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return NotFound("No Person object found for the current user.");
            }

            var nutrition = await _context.Nutrition
                .Include(n => n.Meals)
                .FirstOrDefaultAsync(n => n.PersonId == person.Id);

            if (nutrition == null)
            {
                nutrition = new Nutrition
                {
                    PersonId = person.Id,
                    Meals = new List<Meal>()
                };
                _context.Nutrition.Add(nutrition);
                await _context.SaveChangesAsync();
            }

            var bmr = NutritionCalculator.CalculateBMR(person);
            var averageRoutineCaloriesBurned = NutritionCalculator.CalculateAverageRoutineCaloriesBurned(person);
            var dailyCalorieAdjustment = NutritionCalculator.CalculateDailyCalorieAdjustment(person);
            var totalDailyCalories = NutritionCalculator.GetTotalDailyCalories(person);
            var (proteinPct, carbsPct, fatPct) = NutritionCalculator.CalculateMacroPercentages(person);

            var generatedMeals = await _aiAnalysisService.GenerateMealsFromAI(totalDailyCalories, proteinPct, carbsPct, fatPct);

            _context.Meals.RemoveRange(nutrition.Meals);

            foreach (var meal in generatedMeals)
            {
                meal.NutritionId = nutrition.Id;
            }

            nutrition.Meals = generatedMeals;
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