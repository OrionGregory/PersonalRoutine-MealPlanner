using Assignment3.Models;
using Assignment3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Assignment3.Controllers
{
    [Authorize]
    public class RoutineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIAnalysisService _aiService;
        private readonly UserManager<IdentityUser> _userManager;

        public RoutineController(ApplicationDbContext context, AIAnalysisService aiService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _aiService = aiService;
            _userManager = userManager;
        }

        // GET: Routine
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var routines = await _context.Routines
                .Include(r => r.Exercises) // Include the Exercises property
                .ToListAsync();
            return View(routines);
        }

        // GET: Routine/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var routine = await _context.Routines
                                        .Include(r => r.Person)
                                        .Include(r => r.Exercises)
                                        .FirstOrDefaultAsync(m => m.Id == id && m.Person != null && m.Person.UserId == userId); // Filter by logged-in user
            if (routine == null)
            {
                return NotFound();
            }

            return View(routine);
        }


        // GET: Routine/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return NotFound("Person not found for the logged-in user.");
            }

            var routine = new Routine
            {
                PersonId = person.Id
            };

            return View(routine);
        }

        // POST: Routine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Routine routine)
        {
            var userId = _userManager.GetUserId(User);
            var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
            {
                return NotFound("Person not found for the logged-in user.");
            }

            if (ModelState.IsValid)
            {
                routine.PersonId = person.Id;
                _context.Add(routine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(routine);
        }

        // GET: Routine/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines.FindAsync(id);
            if (routine == null)
            {
                return NotFound();
            }
            ViewBag.Persons = _context.People.ToList();
            return View(routine);
        }

        // POST: Routine/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DayOfWeek,RoutineType,PersonId")] Routine routine)
        {
            if (id != routine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoutineExists(routine.Id))
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
            ViewBag.Persons = _context.People.ToList();
            return View(routine);
        }

        // GET: Routine/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                                        .Include(r => r.Person)
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (routine == null)
            {
                return NotFound();
            }

            return View(routine);
        }

        // POST: Routine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routine = await _context.Routines.FindAsync(id);
            if (routine != null)
            {
                _context.Routines.Remove(routine);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Routine/EditExercises/5
        [HttpGet]
        public async Task<IActionResult> EditExercises(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                .Include(r => r.Exercises)
                .Include(r => r.Person) // Include the Person navigation property
                .FirstOrDefaultAsync(r => r.Id == id);

            if (routine == null)
            {
                return NotFound();
            }

            return View(routine); // Render a view to manage exercises
        }


        // GET: Routine/CreateExercise/5
        [HttpGet]
        public IActionResult CreateExercise(int? routineId)
        {
            if (routineId == null)
            {
                return NotFound();
            }

            var exercise = new Exercise { RoutineId = routineId.Value };
            return View(exercise);
        }

        // POST: Routine/CreateExercise
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateExercise([Bind("Id,Description,Reps,Sets,Name,RoutineId")] Exercise exercise)
{
    if (ModelState.IsValid)
    {
        _context.Add(exercise);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = exercise.RoutineId });
    }
    return View(exercise);
}

        // GET: Routine/EditExercise/5
        [HttpGet]
        public async Task<IActionResult> EditExercise(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }

            return View(exercise);
        }

        // POST: Routine/EditExercise/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditExercise(int id, [Bind("Id,Description,Reps,Sets,Name,RoutineId")] Exercise exercise)
{
    if (id != exercise.Id)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(exercise);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ExerciseExists(exercise.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Details), new { id = exercise.RoutineId });
    }
    return View(exercise);
}

        // POST: Routine/DeleteExercise/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }

            var routineId = exercise.RoutineId;

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = routineId });
        }

        // Helper Methods

        private bool RoutineExists(int id)
        {
            return _context.Routines.Any(e => e.Id == id);
        }

        private bool ExerciseExists(int id)
        {
            return _context.Exercises.Any(e => e.Id == id);
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

        private List<Exercise> GetPredefinedExercises(string routineType, int routineId)
        {
            var exercises = new List<Exercise>();

            switch (routineType.ToLower())
            {
                case "push":
                    exercises.Add(new Exercise { Description = "Bench Press", Sets = 3, Reps = 10, Name = "Bench Press", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Overhead Press", Sets = 3, Reps = 8, Name = "Overhead Press", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Tricep Dips", Sets = 3, Reps = 12, Name = "Tricep Dips", RoutineId = routineId });
                    break;
                case "pull":
                    exercises.Add(new Exercise { Description = "Deadlift", Sets = 3, Reps = 5, Name = "Deadlift", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Pull-Ups", Sets = 3, Reps = 10, Name = "Pull-Ups", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Bicep Curls", Sets = 3, Reps = 12, Name = "Bicep Curls", RoutineId = routineId });
                    break;
                case "legs":
                    exercises.Add(new Exercise { Description = "Squats", Sets = 3, Reps = 10, Name = "Squats", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Leg Press", Sets = 3, Reps = 12, Name = "Leg Press", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Lunges", Sets = 3, Reps = 15, Name = "Lunges", RoutineId = routineId });
                    break;
                case "cardio":
                    exercises.Add(new Exercise { Description = "Running", Sets = 1, Reps = 30, Name = "Running", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Cycling", Sets = 1, Reps = 30, Name = "Cycling", RoutineId = routineId });
                    break;
                case "full body":
                    exercises.Add(new Exercise { Description = "Burpees", Sets = 3, Reps = 15, Name = "Burpees", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Kettlebell Swings", Sets = 3, Reps = 20, Name = "Kettlebell Swings", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Plank", Sets = 3, Reps = 60, Name = "Plank", RoutineId = routineId });
                    break;
                case "cardio and core":
                    exercises.Add(new Exercise { Description = "Mountain Climbers", Sets = 3, Reps = 30, Name = "Mountain Climbers", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Russian Twists", Sets = 3, Reps = 20, Name = "Russian Twists", RoutineId = routineId });
                    exercises.Add(new Exercise { Description = "Jump Rope", Sets = 3, Reps = 5, Name = "Jump Rope (minutes)", RoutineId = routineId });
                    break;
                case "rest":
                    exercises.Add(new Exercise { Description = "Stretching", Sets = 1, Reps = 15, Name = "Stretching", RoutineId = routineId });
                    break;
                default:
                    // Optionally, add default exercises or leave empty
                    break;
            }

            return exercises;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwapRoutines([FromBody] SwapRoutineModel model)
        {
            if (model == null || model.RoutineId <= 0 || string.IsNullOrEmpty(model.TargetDay))
            {
                return BadRequest(new { message = "Invalid data provided." });
            }

            var routine = await _context.Routines.FindAsync(model.RoutineId);

            if (routine == null)
            {
                return NotFound(new { message = "Routine not found." });
            }

            routine.DayOfWeek = model.TargetDay;
            _context.Update(routine);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateRoutine(int id)
        {
            // Fetch routine with related data
            var routine = await _context.Routines
                .Include(r => r.Exercises)
                .Include(r => r.Person)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (routine == null)
            {
                Console.WriteLine($"RegenerateRoutine POST: Routine with ID {id} not found.");
                return NotFound();
            }

            try
            {
                // Remove existing exercises if any
                if (routine.Exercises != null && routine.Exercises.Any())
                {
                    Console.WriteLine($"RegenerateRoutine POST: Removing existing exercises for Routine ID {id}.");
                    _context.Exercises.RemoveRange(routine.Exercises);
                }

                List<Exercise> newExercises;

                try
                {
                    // Prepare data for AI request
                    var currentWeight = routine.Person?.Weight ?? 0;
                    var goalWeight = routine.Person?.GoalWeight ?? 0;
                    var timeFrame = 12; // Example default timeframe in weeks

                    Console.WriteLine($"RegenerateRoutine POST: Generating exercises with RoutineType: {routine.RoutineType}, " +
                                      $"CurrentWeight: {currentWeight}, GoalWeight: {goalWeight}, TimeFrame: {timeFrame}.");

                    // Attempt to generate exercises using AI
                    if (string.IsNullOrEmpty(routine.RoutineType))
                    {
                        throw new Exception("RoutineType is null or empty.");
                    }

                    newExercises = await _aiService.GenerateExercisesFromAI(
                        routine.RoutineType,
                        currentWeight,
                        goalWeight,
                        timeFrame
                    );

                    if (newExercises == null || !newExercises.Any())
                    {
                        throw new Exception("AI returned no exercises.");
                    }
                }
                catch (Exception ex)
                {
                    // Log AI error and use predefined exercises
                    Console.WriteLine($"RegenerateRoutine POST: AI error or no response for Routine ID {id}: {ex.Message}");
                    if (string.IsNullOrEmpty(routine.RoutineType))
                    {
                        throw new Exception("RoutineType is null or empty.");
                    }
                    newExercises = GetPredefinedExercises(routine.RoutineType, routine.Id);

                    if (newExercises == null || !newExercises.Any())
                    {
                        Console.WriteLine($"RegenerateRoutine POST: No predefined exercises available for RoutineType: {routine.RoutineType}");
                        TempData["AIError"] = "Error generating exercises. Please try again.";
                        return RedirectToAction(nameof(Details), new { id = routine.Id });
                    }
                }

                // Associate exercises with the routine and save to database
                foreach (var exercise in newExercises)
                {
                    exercise.RoutineId = routine.Id;
                }

                _context.Exercises.AddRange(newExercises);
                await _context.SaveChangesAsync();
                Console.WriteLine($"RegenerateRoutine POST: Successfully regenerated exercises for Routine ID {id}.");
            }
            catch (Exception ex)
            {
                // Log error details and notify the user
                Console.WriteLine($"RegenerateRoutine POST: Unexpected error for Routine ID {id}: {ex.Message}");
                TempData["AIError"] = "Unexpected error while regenerating exercises.";
                return RedirectToAction(nameof(Details), new { id = routine.Id });
            }

            // Redirect to the details view of the routine
            return RedirectToAction(nameof(Details), new { id = routine.Id });
        }



        public class SwapRoutineModel
        {
            public int RoutineId { get; set; }
            public string? TargetDay { get; set; }
        }

    }
}
