using System.Threading.Tasks;
using Assignment3.Data;
using Assignment3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Controllers
{
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonController> _logger;

        public PersonController(ApplicationDbContext context, ILogger<PersonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Person/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created Person with ID {person.Id}");

                // Generate Routine
                var routine = GenerateRoutine(person);

                // Save Routine
                _context.Routines.Add(routine);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = person.Id });
            }

            _logger.LogWarning("Invalid ModelState in Create Person");
            return View(person);
        }

        // GET: Person/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details GET: No id provided.");
                return NotFound();
            }

            var person = await _context.People
                .Include(p => p.Routine)
                    .ThenInclude(r => r.Exercises)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                _logger.LogWarning($"Details GET: Person with id {id} not found.");
                return NotFound();
            }

            return View(person);
        }

        // Helper method to generate a routine based on person details
        private Routine GenerateRoutine(Person person)
        {
            var exercises = new List<Exercise>();

            // Simple logic: if goal is to lose weight, add cardio exercises
            if (person.goalWeight < person.weight)
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
