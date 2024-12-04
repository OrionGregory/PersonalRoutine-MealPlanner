using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using ClassDemo.Data;

namespace Assignment3.Controllers
{
    [Authorize] // Ensures only authenticated users can access these actions
    public class RoutineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIAnalysisService _aiService;

        public RoutineController(ApplicationDbContext context, AIAnalysisService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // GET: Routine
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var routines = _context.Routines
                                    .Include(r => r.Person)
                                    .Include(r => r.Exercises); // Include exercises for display
            return View(await routines.ToListAsync());
        }

        // GET: Routine/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                .Include(r => r.Person)
                .Include(r => r.Exercises)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (routine == null)
            {
                return NotFound();
            }

            return View(routine);
        }


        // GET: Routine/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Optionally, pass any necessary data to the view, e.g., list of persons
            ViewBag.Persons = _context.People.ToList();
            return View();
        }

        // POST: Routine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DayOfWeek,RoutineType,PersonId")] Routine routine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(routine);
                await _context.SaveChangesAsync();

                // Pre-populate exercises based on RoutineType
                var predefinedExercises = GetPredefinedExercises(routine.RoutineType, routine.Id);
                if (predefinedExercises != null && predefinedExercises.Any())
                {
                    _context.Exercises.AddRange(predefinedExercises);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Persons = _context.People.ToList();
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
                return RedirectToAction(nameof(EditExercises), new { id = exercise.RoutineId });
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
                return RedirectToAction(nameof(EditExercises), new { id = exercise.RoutineId });
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

            return RedirectToAction(nameof(EditExercises), new { id = routineId });
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
            public string TargetDay { get; set; }
        }

    }
}
