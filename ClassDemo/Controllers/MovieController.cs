// Controllers/MovieController.cs
using System.Linq;
using System.Threading.Tasks;
using ClassDemo.Data;
using ClassDemo.Models;
using ClassDemo.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassDemo.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIAnalysisService _aiService;
        private readonly ILogger<MovieController> _logger;

        public MovieController(ApplicationDbContext context, AIAnalysisService aiService, ILogger<MovieController> logger)
        {
            _context = context;
            _aiService = aiService;
            _logger = logger;
        }

        // GET: Movie
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .ToListAsync();

            ViewData["Title"] = "Movies Index";
            _logger.LogInformation("Index: Retrieved and displayed list of movies.");

            return View(movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.AIReviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                _logger.LogWarning($"Details: Movie with id {id} not found.");
                return NotFound();
            }

            // Assuming MovieDetailsViewModel has these properties
            var viewModel = new MovieDetailsViewModel
            {
                Title = movie.Title,
                Movie = movie,
                Actors = movie.MovieActors?.Select(ma => ma.Actor).ToList(),
                AIReviews = movie.AIReviews?.ToList(),
                OverallSentiment = movie.AIReviews != null && movie.AIReviews.Any()
                    ? movie.AIReviews.GroupBy(r => r.Sentiment)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "No Sentiment"
                    : "Unknown"
            };

            return View(viewModel);  // Pass the correct view model
        }


        // GET: Movie/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Movie";
            return View(new Movie());
        }

        // POST: Movie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Create: Movie '{movie.Title}' created successfully.");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Create: Model state is invalid.");
            ViewData["Title"] = "Create Movie";
            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit: No id provided.");
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                _logger.LogWarning($"Edit: Movie with id {id} not found.");
                return NotFound();
            }

            ViewData["Title"] = $"Edit Movie - {movie.Title}";
            return View(movie);
        }

        // POST: Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.Id)
            {
                _logger.LogWarning($"Edit: Mismatch between route id {id} and movie id {movie.Id}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Edit: Movie '{movie.Title}' updated successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MovieExists(movie.Id))
                    {
                        _logger.LogWarning($"Edit: Movie with id {movie.Id} does not exist.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, $"Edit: Concurrency error while updating movie id {movie.Id}.");
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Edit: Model state is invalid.");
            ViewData["Title"] = $"Edit Movie - {movie.Title}";
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete: No id provided.");
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                _logger.LogWarning($"Delete: Movie with id {id} not found.");
                return NotFound();
            }

            ViewData["Title"] = $"Delete Movie - {movie.Title}";
            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"DeleteConfirmed: Movie '{movie.Title}' deleted successfully.");
            }
            else
            {
                _logger.LogWarning($"DeleteConfirmed: Movie with id {id} not found.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a movie exists
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAIReviews(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.AIReviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                _logger.LogWarning($"GenerateAIReviews: Movie with id {id} not found.");
                return NotFound();
            }

            var prompt = $@"
Generate 10 short, one-sentence reviews for the movie '{movie.Title}'. 
For each review, include a sentiment label ('Positive', 'Negative', or 'Neutral'). 
Output the reviews as a JSON array where each element is an object with 'review' and 'sentiment' fields.";

            try
            {
                var aiReviews = await _aiService.GetChatGPTResponseAsync<AIReview>(prompt);

                if (aiReviews == null || !aiReviews.Any())
                {
                    TempData["AIError"] = "AI did not return any reviews.";
                    _logger.LogWarning($"GenerateAIReviews: AI did not return any reviews for movie id {id}.");
                    return RedirectToAction("Details", new { id });
                }

                _context.AIReviews.RemoveRange(movie.AIReviews);

                foreach (var review in aiReviews)
                {
                    review.MovieId = movie.Id;
                    _context.AIReviews.Add(review);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"GenerateAIReviews: Successfully generated and saved AI reviews for movie id {id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GenerateAIReviews: Error generating AI reviews for movie id {id}.");
                TempData["AIError"] = "Error generating AI reviews: " + ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
