// Controllers/MovieActorController.cs
using System.Linq;
using System.Threading.Tasks;
using ClassDemo.Data;
using ClassDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassDemo.Controllers
{
    public class MovieActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovieActorController> _logger;

        public MovieActorController(ApplicationDbContext context, ILogger<MovieActorController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: MovieActor/Create
        public IActionResult Create()
        {
            ViewData["Movies"] = _context.Movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            }).ToList();

            ViewData["Actors"] = _context.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            return View(new MovieActor());
        }

        // POST: MovieActor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieActor movieActor)
        {
            if (ModelState.IsValid)
            {
                bool alreadyExists = await _context.MovieActors
                    .AnyAsync(ma => ma.MovieId == movieActor.MovieId && ma.ActorId == movieActor.ActorId);

                if (!alreadyExists)
                {
                    _context.Add(movieActor);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Create POST: Successfully created MovieActor link between MovieId {movieActor.MovieId} and ActorId {movieActor.ActorId}.");
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "This actor is already linked to the movie.");
            }

            ViewData["Movies"] = _context.Movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            }).ToList();

            ViewData["Actors"] = _context.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            _logger.LogWarning("Create POST: Model state is invalid or duplicate link attempted.");
            return View(movieActor);
        }

        public async Task<IActionResult> Index()
        {
            var movieActors = _context.MovieActors
                .Include(ma => ma.Movie)
                .Include(ma => ma.Actor);
            return View(await movieActors.ToListAsync());
        }

        // GET: MovieActor/Edit
        public async Task<IActionResult> Edit(int? movieId, int? actorId)
        {
            if (movieId == null || actorId == null)
                return NotFound();

            var movieActor = await _context.MovieActors
                .Include(ma => ma.Movie)
                .Include(ma => ma.Actor)
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor == null)
                return NotFound();

            ViewData["Movies"] = new SelectList(_context.Movies, "Id", "Title", movieActor.MovieId);
            ViewData["Actors"] = new SelectList(_context.Actors, "Id", "Name", movieActor.ActorId);

            return View(movieActor);
        }

        // POST: MovieActor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int movieId, int actorId, [Bind("MovieId,ActorId")] MovieActor movieActor)
        {
            if (movieId != movieActor.MovieId || actorId != movieActor.ActorId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieActor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieActorExists((int)movieActor.MovieId, (int)movieActor.ActorId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Movies"] = new SelectList(_context.Movies, "Id", "Title", movieActor.MovieId);
            ViewData["Actors"] = new SelectList(_context.Actors, "Id", "Name", movieActor.ActorId);
            return View(movieActor);
        }

        private bool MovieActorExists(int movieId, int actorId)
        {
            return _context.MovieActors.Any(e => e.MovieId == movieId && e.ActorId == actorId);
        }



        // GET: MovieActor/Delete
        public async Task<IActionResult> Delete(int movieId, int actorId)
        {
            var movieActor = await _context.MovieActors
                .Include(ma => ma.Movie)
                .Include(ma => ma.Actor)
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor == null)
            {
                _logger.LogWarning($"Delete GET: MovieActor with MovieId {movieId} and ActorId {actorId} not found.");
                return NotFound();
            }

            return View(movieActor);
        }

        // POST: MovieActor/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int movieId, int actorId)
        {
            var movieActor = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == movieId && ma.ActorId == actorId);

            if (movieActor != null)
            {
                _context.MovieActors.Remove(movieActor);
                _logger.LogInformation($"DeleteConfirmed: Successfully deleted MovieActor with MovieId {movieId} and ActorId {actorId}.");
            }
            else
            {
                _logger.LogWarning($"DeleteConfirmed: MovieActor with MovieId {movieId} and ActorId {actorId} not found.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
