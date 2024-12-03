using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment3.Data;
using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Controllers
{
    [Authorize] // Ensure only authenticated users can access these actions
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public PersonController(ApplicationDbContext context, ILogger<PersonController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Person/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                .Include(p => p.Routine)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                _logger.LogWarning($"Edit GET: No Person found for UserId {userId}. Redirecting to Create.");
                return RedirectToAction(nameof(Create));
            }

            return View(person);
        }

        // POST: Person/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Person person)
        {
            var userId = _userManager.GetUserId(User);
            var existingPerson = await _context.People.FirstOrDefaultAsync(p => p.Id == person.Id && p.UserId == userId);

            if (existingPerson == null)
            {
                _logger.LogWarning($"Edit POST: No Person found with ID {person.Id} for UserId {userId}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update fields
                    existingPerson.Name = person.Name;
                    existingPerson.Age = person.Age;
                    existingPerson.Sex = person.Sex;
                    existingPerson.Weight = person.Weight;
                    existingPerson.GoalWeight = person.GoalWeight;
                    existingPerson.Time = person.Time;

                    _context.Update(existingPerson);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully updated Person with ID {existingPerson.Id}.");

                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Edit POST: Concurrency error while updating Person with ID {person.Id}: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }

            _logger.LogWarning("Edit POST: ModelState is invalid.");
            return View(person);
        }


        // GET: Person/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation($"Attempting to create Person for UserId: {userId}");
            person.UserId = userId;

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid. Adding Person to context.");
                _context.Add(person);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully saved Person with ID {person.Id}");

                    // Generate and save Routine
                    var routine = GenerateRoutine(person);
                    _context.Routines.Add(routine);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully saved Routine with ID {routine.Id}");

                    return RedirectToAction(nameof(Details));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving Person or Routine: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving your profile. Please try again.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid. Returning to Create view.");
            }

            return View(person);
        }



        // GET: Person/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation($"Details GET: UserId = {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Details GET: User is not authenticated.");
                return RedirectToAction("Login", "Account"); // Adjust the redirect as needed
            }

            var person = await _context.People
                .Include(p => p.Routine)
                    .ThenInclude(r => r.Exercises)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                _logger.LogWarning($"Details GET: No Person found for UserId {userId}. Redirecting to Create.");
                return RedirectToAction(nameof(Create));
            }

            _logger.LogInformation($"Details GET: Retrieved Person with ID {person.Id}");
            return View(person);
        }


        // Helper method to generate a routine based on person details
        private Routine GenerateRoutine(Person person)
        {
            var exercises = new List<Exercise>();

            // Simple logic: if goal is to lose weight, add cardio exercises
            if (person.GoalWeight < person.Weight)
            {
                exercises.Add(new Exercise
                {
                    Description = "Running",
                    Reps = 0,
                    Sets = 0
                });
                exercises.Add(new Exercise
                {
                    Description = "Cycling",
                    Reps = 0,
                    Sets = 0
                });
            }
            else // Goal is to gain weight or maintain
            {
                exercises.Add(new Exercise
                {
                    Description = "Bench Press",
                    Reps = 10,
                    Sets = 3
                });
                exercises.Add(new Exercise
                {
                    Description = "Squats",
                    Reps = 12,
                    Sets = 3
                });
            }

            return new Routine
            {
                PersonId = person.Id,
                Exercises = exercises
            };
        }
    }
}
