using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassDemo.Data;
using Assignment3.Data;
using System;

namespace Assignment3.Controllers
{
    [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                .Include(p => p.Routines)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                _logger.LogWarning($"Edit GET: No Person found for UserId {userId}. Redirecting to Create.");
                return RedirectToAction(nameof(Create));
            }

            return View(person);
        }

        [HttpGet]
        public async Task<IActionResult> AdminMenu()
        {
            return View(await _context.People.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AdminEdit(Person person)
        {
            var userId = _userManager.GetUserId(User);
            var existingPerson = await _context.People
                .Include(p => p.Routines)
                .FirstOrDefaultAsync(p => p.Id == person.Id && p.UserId == userId);

            if (existingPerson == null)
            {
                _logger.LogWarning($"Edit POST: No Person found with ID {person.Id} for UserId {userId}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPerson.Name = person.Name;
                    existingPerson.Age = person.Age;
                    existingPerson.Sex = person.Sex;
                    existingPerson.Weight = person.Weight;
                    existingPerson.GoalWeight = person.GoalWeight;
                    existingPerson.Time = person.Time;

                    if(person.isAdmin == null) {
                        existingPerson.isAdmin = false;
                    }
                    if(person.isAdmin == true)
                    {
                        existingPerson.isAdmin = true;
                    }
                    if(person.isAdmin == false)
                    {
                        existingPerson.isAdmin = false;
                    }

                    _context.Update(existingPerson);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Edit POST: Concurrency error while updating Person with ID {person.Id}: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }

            return View(person);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Person person)
        {
            var userId = _userManager.GetUserId(User);
            var existingPerson = await _context.People
                .Include(p => p.Routines)
                .FirstOrDefaultAsync(p => p.Id == person.Id && p.UserId == userId);

            if (existingPerson == null)
            {
                _logger.LogWarning($"Edit POST: No Person found with ID {person.Id} for UserId {userId}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPerson.Name = person.Name;
                    existingPerson.Age = person.Age;
                    existingPerson.Sex = person.Sex;
                    existingPerson.Weight = person.Weight;
                    existingPerson.GoalWeight = person.GoalWeight;
                    existingPerson.Time = person.Time;

                    _context.Update(existingPerson);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Edit POST: Concurrency error while updating Person with ID {person.Id}: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }

            return View(person);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            var userId = _userManager.GetUserId(User);
            person.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(person);
                try
                {
                    await _context.SaveChangesAsync();

                    var routines = GenerateRoutine(person);
                    foreach (var routine in routines)
                    {
                        routine.PersonId = person.Id;
                        _context.Routines.Add(routine);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving Person or Routine: {ex.Message}");
                }
            }

            return View(person);
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var userId = _userManager.GetUserId(User);

            var person = await _context.People
                .Include(p => p.Routines!)
                    .ThenInclude(r => r.Exercises!)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(person);
        }

        private List<Routine> GenerateRoutine(Person person)
        {
            var routines = new List<Routine>();

            // Example for gaining weight
            routines.Add(new Routine { DayOfWeek = "Monday", RoutineType = "Push" });
            routines.Add(new Routine { DayOfWeek = "Tuesday", RoutineType = "Pull" });
            routines.Add(new Routine { DayOfWeek = "Wednesday", RoutineType = "Legs" });
            routines.Add(new Routine { DayOfWeek = "Thursday", RoutineType = "Rest" });
            routines.Add(new Routine { DayOfWeek = "Friday", RoutineType = "Full Body" });
            routines.Add(new Routine { DayOfWeek = "Saturday", RoutineType = "Cardio and Core" });
            routines.Add(new Routine { DayOfWeek = "Sunday", RoutineType = "Rest" });

            return routines;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateRoutine(int id)
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                .Include(p => p.Routines)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (person == null)
            {
                return NotFound();
            }

            try
            {
                if (person.Routines != null)
                {
                    _context.Routines.RemoveRange(person.Routines);
                    await _context.SaveChangesAsync();
                }

                var newRoutines = GenerateRoutine(person);
                foreach (var routine in newRoutines)
                {
                    routine.PersonId = person.Id;
                    _context.Routines.Add(routine);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error regenerating routines: {ex.Message}");
                return RedirectToAction(nameof(Details));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwapRoutines([FromBody] RoutineSwapRequest request)
        {
            if (request == null || request.RoutineId == 0 || string.IsNullOrEmpty(request.TargetDay))
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            // Retrieve the dragged routine
            var draggedRoutine = await _context.Routines.FirstOrDefaultAsync(r => r.Id == request.RoutineId);
            if (draggedRoutine == null || request.TargetDay == null)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            if (draggedRoutine == null)
            {
                return NotFound(new { message = "Dragged routine not found." });
            }

            // Retrieve the routine on the target day (if it exists)
            var targetRoutine = await _context.Routines
                .FirstOrDefaultAsync(r => r.DayOfWeek != null && request.TargetDay != null && 
                    r.DayOfWeek.ToLower() == request.TargetDay.ToLower() && r.PersonId == draggedRoutine.PersonId);

            if (targetRoutine != null)
            {
                // Swap the days between the routines
                var tempDay = draggedRoutine.DayOfWeek;
                draggedRoutine.DayOfWeek = targetRoutine.DayOfWeek;
                targetRoutine.DayOfWeek = tempDay;
            }
            else
            {
                // If no routine exists on the target day, just move the dragged routine
                draggedRoutine.DayOfWeek = request.TargetDay;
            }

            // Save changes to the database
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Routines swapped successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving routine swaps: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while saving routine swaps." });
            }
        }
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateWeight(int id, float newWeight)
{
    var userId = _userManager.GetUserId(User);
    var person = await _context.People
        .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    if (person == null)
    {
        return NotFound();
    }

    person.Weight = newWeight;
    _context.Update(person);
    await _context.SaveChangesAsync();

    return RedirectToAction("Index", "Home");
}


        public class RoutineSwapRequest
        {
            public int RoutineId { get; set; }
            public string? TargetDay { get; set; }
        }


    }
}
