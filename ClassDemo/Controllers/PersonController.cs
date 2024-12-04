using Assignment3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassDemo.Data;
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

        // GET: Person/Edit
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation($"Attempting to create Person for UserId: {userId}");
            person.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(person);
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully saved Person with ID {person.Id}");

                    // Generate and save Routines
                    var routines = GenerateRoutine(person);
                    foreach (var routine in routines)
                    {
                        routine.PersonId = person.Id;
                        _context.Routines.Add(routine);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully saved all generated routines.");

                    return RedirectToAction(nameof(Details));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving Person or Routine: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving your profile. Please try again.");
                }
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
                return RedirectToAction("Login", "Account");
            }

            var person = await _context.People
                .Include(p => p.Routines)
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

        private List<Routine> GenerateRoutine(Person person)
        {
            var routines = new List<Routine>();

            if (person.GoalWeight > person.Weight) // Gaining Weight
            {
                // Push Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Monday",
                    RoutineType = "Push",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Bench Press", Reps = 10, Sets = 3 },
                new Exercise { Description = "Shoulder Press", Reps = 12, Sets = 3 },
                new Exercise { Description = "Tricep Dips", Reps = 15, Sets = 3 }
            }
                });

                // Pull Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Tuesday",
                    RoutineType = "Pull",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Pull-Ups", Reps = 8, Sets = 3 },
                new Exercise { Description = "Barbell Rows", Reps = 10, Sets = 3 },
                new Exercise { Description = "Bicep Curls", Reps = 15, Sets = 3 }
            }
                });

                // Leg Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Wednesday",
                    RoutineType = "Legs",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Squats", Reps = 12, Sets = 3 },
                new Exercise { Description = "Lunges", Reps = 10, Sets = 3 },
                new Exercise { Description = "Calf Raises", Reps = 20, Sets = 3 }
            }
                });

                // Rest Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Thursday",
                    RoutineType = "Rest",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Light Stretching", Reps = 0, Sets = 15 }
            }
                });

                // Full Body Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Friday",
                    RoutineType = "Full Body",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Deadlifts", Reps = 10, Sets = 3 },
                new Exercise { Description = "Push-Ups", Reps = 15, Sets = 3 },
                new Exercise { Description = "Plank", Reps = 1, Sets = 3 } // Hold for 1 minute
            }
                });

                // Cardio and Core
                routines.Add(new Routine
                {
                    DayOfWeek = "Saturday",
                    RoutineType = "Cardio and Core",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Jogging", Reps = 0, Sets = 30 }, // 30 minutes
                new Exercise { Description = "Ab Rollouts", Reps = 15, Sets = 3 }
            }
                });

                // Rest Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Sunday",
                    RoutineType = "Rest",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Foam Rolling", Reps = 0, Sets = 15 }
            }
                });
            }
            else // Losing Weight or Maintenance
            {
                // Cardio
                routines.Add(new Routine
                {
                    DayOfWeek = "Monday",
                    RoutineType = "Cardio",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Running", Reps = 0, Sets = 30 } // 30 minutes
            }
                });

                // Strength Training
                routines.Add(new Routine
                {
                    DayOfWeek = "Tuesday",
                    RoutineType = "Strength",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Push-Ups", Reps = 20, Sets = 3 },
                new Exercise { Description = "Squats", Reps = 15, Sets = 3 },
                new Exercise { Description = "Plank", Reps = 1, Sets = 3 } // Hold for 1 minute
            }
                });

                // Cardio and Core
                routines.Add(new Routine
                {
                    DayOfWeek = "Wednesday",
                    RoutineType = "Cardio and Core",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Cycling", Reps = 0, Sets = 30 }, // 30 minutes
                new Exercise { Description = "Sit-Ups", Reps = 15, Sets = 3 }
            }
                });

                // Rest Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Thursday",
                    RoutineType = "Rest",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Yoga", Reps = 0, Sets = 30 } // 30 minutes
            }
                });

                // Full Body Strength
                routines.Add(new Routine
                {
                    DayOfWeek = "Friday",
                    RoutineType = "Full Body Strength",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Burpees", Reps = 15, Sets = 3 },
                new Exercise { Description = "Lunges", Reps = 12, Sets = 3 },
                new Exercise { Description = "Plank", Reps = 1, Sets = 3 }
            }
                });

                // Cardio
                routines.Add(new Routine
                {
                    DayOfWeek = "Saturday",
                    RoutineType = "Cardio",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Swimming", Reps = 0, Sets = 30 } // 30 minutes
            }
                });

                // Rest Day
                routines.Add(new Routine
                {
                    DayOfWeek = "Sunday",
                    RoutineType = "Rest",
                    Exercises = new List<Exercise>
            {
                new Exercise { Description = "Stretching", Reps = 0, Sets = 15 }
            }
                });
            }

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
                _logger.LogWarning($"RegenerateRoutine POST: No Person found with ID {id} for UserId {userId}.");
                return NotFound();
            }

            try
            {
                // Remove existing routines
                if (person.Routines != null)
                {
                    _context.Routines.RemoveRange(person.Routines);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Deleted existing routines for Person with ID {id}.");
                }

                // Generate and save new routines
                var newRoutines = GenerateRoutine(person);
                foreach (var routine in newRoutines)
                {
                    routine.PersonId = person.Id;
                    _context.Routines.Add(routine);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully regenerated routines for Person with ID {id}.");

                return RedirectToAction(nameof(Details));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error regenerating routines for Person with ID {id}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while regenerating your workout plan. Please try again.");
                return RedirectToAction(nameof(Details), new { id = person.Id });
            }
        }
    }
}
