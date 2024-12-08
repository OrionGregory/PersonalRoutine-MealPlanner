using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

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

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                    .Include(p => p.Routines)
                    .ThenInclude(r => r.Exercises)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var completedExercises = await _context.CompletedExercises
                .Where(ce => ce.UserId == userId && ce.CompletedDate.Date == DateTime.Today)
                .Select(ce => ce.ExerciseId)
                .ToListAsync();

            ViewBag.CompletedExercises = completedExercises;

            return View(person);
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
        public async Task<IActionResult> Delete(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        public async Task<IActionResult> EditWeightHistory(int id)
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null || person.weight_history == null)
            {
                _logger.LogWarning($"Edit GET: No Person found for UserId {userId}. Redirecting to Create.");
                return RedirectToAction(nameof(Index));
            }

            return View(person);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WeightDelete(int entry_id, int person_id)
        {
            // Load the person along with their weight history
            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Id == person_id);

            if (person != null && person.weight_history != null && person.weight_history.Count > 0)
            {
                // Validate the entry_id to ensure it's within valid bounds
                if (entry_id >= 0 && entry_id < person.weight_history.Count)
                {
                    // Remove the entry from the weight_history list at the given index
                    person.weight_history.RemoveAt(entry_id);
                    _context.Update(person);  // Update the person entity in the context
                    await _context.SaveChangesAsync();  // Save changes to the database
                }
                else
                {
                    // Handle invalid index (optional logging or error message)
                    _logger.LogWarning($"Invalid entry_id {entry_id} for person with Id {person_id}. Index out of bounds.");
                }
            }

            // Redirect back to the 'Home/Index' page after deletion
            return RedirectToAction(nameof(Index));
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person != null)
            {
                _context.People.Remove(person);
                _context.Update(person);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AdminMenu()
        {
            return View(await _context.People.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> AdminEdit(int id)
        {
            var person = await _context.People
                .Include(p => p.Routines)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                _logger.LogWarning($"Edit GET: No Person found for UserId {id}. Redirecting to Create.");
                return RedirectToAction(nameof(Create));
            }

            return View(person);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminEdit(Person person)
        {
            var existingPerson = await _context.People
                .FirstOrDefaultAsync(p => p.Id.ToString() == person.Id.ToString());
            if (existingPerson == null)
            {
                return View(person);
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
                    existingPerson.isAdmin = person.isAdmin ?? false;

                    _context.Update(existingPerson);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(AdminMenu));
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
                    return RedirectToAction(nameof(Index));
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
            person.User = await _userManager.GetUserAsync(User);

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

    if(person.weight_history == null)
    {
       person.weight_history = [];
    }
    person.weight_history.Add(newWeight);
    person.Weight = newWeight;
    _context.Update(person);
    await _context.SaveChangesAsync();

    return RedirectToAction("Index", "Home");
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleExerciseCompletion([FromBody] ExerciseCompletionModel model)
{
    var userId = _userManager.GetUserId(User);
    
    if (model.completed)
    {
        var completedExercise = new CompletedExercise
        {
            ExerciseId = model.exerciseId,
            UserId = userId,
            CompletedDate = DateTime.Today
        };
        _context.CompletedExercises.Add(completedExercise);
    }
    else
    {
        var completedExercise = await _context.CompletedExercises
            .FirstOrDefaultAsync(ce => ce.ExerciseId == model.exerciseId && 
                                     ce.UserId == userId && 
                                     ce.CompletedDate.Date == DateTime.Today);
        if (completedExercise != null)
        {
            _context.CompletedExercises.Remove(completedExercise);
        }
    }
    
    await _context.SaveChangesAsync();
    return Json(new { success = true });
}

public class ExerciseCompletionModel
{
    public int exerciseId { get; set; }
    public bool completed { get; set; }
}

        public class RoutineSwapRequest
        {
            public int RoutineId { get; set; }
            public string? TargetDay { get; set; }
        }


    }
}
