using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using Assignment3.Models;

namespace Assignment3.Controllers
{
    public class RoutineController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoutineController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Routine
        public async Task<IActionResult> Index()
        {
            var routines = _context.Routines.Include(r => r.Person);
            return View(await routines.ToListAsync());
        }

        // GET: Routine/Details/5
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

            return View(routine); // Ensure this matches the Details.cshtml view
        }


        // GET: Routine/Create
        public IActionResult Create()
        {
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
                return RedirectToAction(nameof(Index));
            }
            return View(routine);
        }

        // GET: Routine/Edit/5
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
            return View(routine);
        }

        // GET: Routine/Delete/5
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
            _context.Routines.Remove(routine);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoutineExists(int id)
        {
            return _context.Routines.Any(e => e.Id == id);
        }
        // GET: Routine/EditExercises/5
        public async Task<IActionResult> EditExercises(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                .Include(r => r.Exercises)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (routine == null)
            {
                return NotFound();
            }

            return View(routine); // Render a view to manage exercises
        }

        // POST: Routine/AddExercise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExercise(int routineId, Exercise exercise)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(EditExercises), new { id = routineId });
            }

            var routine = await _context.Routines
                .Include(r => r.Exercises)
                .FirstOrDefaultAsync(r => r.Id == routineId);

            if (routine == null)
            {
                return NotFound();
            }

            routine.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EditExercises), new { id = routineId });
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

    }
}
