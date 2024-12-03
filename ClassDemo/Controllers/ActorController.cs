// Controllers/ActorController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassDemo.Data;
using ClassDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassDemo.Controllers
{
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AIAnalysisService _aiService;
        private readonly ILogger<ActorController> _logger;

        public ActorController(ApplicationDbContext context, AIAnalysisService aiService, ILogger<ActorController> logger)
        {
            _context = context;
            _aiService = aiService;
            _logger = logger;
        }

        // GET: Actor
        public async Task<IActionResult> Index()
        {
            var actors = await _context.Actors.ToListAsync();
            return View(actors);
        }

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details GET: No id provided.");
                return NotFound();
            }

            var actor = await _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .Include(a => a.ActorTweets)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                _logger.LogWarning($"Details GET: Actor with id {id} not found.");
                return NotFound();
            }

            var movies = actor.MovieActors?.Select(ma => ma.Movie).ToList() ?? new List<Movie>();
            var tweets = actor.ActorTweets?.ToList() ?? new List<ActorTweet>();

            string overallSentiment = "Unknown";
            if (tweets.Any())
            {
                overallSentiment = CalculateOverallSentiment(tweets);
            }

            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = movies,
                TweetSentiments = tweets,
                OverallSentiment = overallSentiment
            };

            return View(viewModel);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Gender,Age,IMDBLink")] Actor actor, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new System.IO.MemoryStream();
                    await photo.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Create POST: Successfully created actor {actor.Id}.");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Create POST: Model state is invalid.");
            return View(actor);
        }

        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit GET: No id provided.");
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                _logger.LogWarning($"Edit GET: Actor with id {id} not found.");
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile? photo)
        {
            if (id != actor.Id)
            {
                _logger.LogWarning($"Edit POST: Mismatch between route id {id} and actor id {actor.Id}.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle photo upload
                    if (photo != null && photo.Length > 0)
                    {
                        using var memoryStream = new System.IO.MemoryStream();
                        await photo.CopyToAsync(memoryStream);
                        actor.Photo = memoryStream.ToArray();
                        _logger.LogInformation($"Edit POST: New photo uploaded for actor {actor.Id}.");
                    }
                    else
                    {
                        // Retain existing photo if no new photo is uploaded
                        var existingActor = await _context.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                        if (existingActor != null)
                        {
                            actor.Photo = existingActor.Photo;
                            _logger.LogInformation($"Edit POST: Retaining existing photo for actor {actor.Id}.");
                        }
                        else
                        {
                            _logger.LogWarning($"Edit POST: Existing actor with id {id} not found for retaining photo.");
                        }
                    }

                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Edit POST: Successfully updated actor {actor.Id}.");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ActorExists(actor.Id))
                    {
                        _logger.LogWarning($"Edit POST: Actor with id {actor.Id} not found during concurrency check.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, $"Edit POST: Concurrency error while updating actor {actor.Id}.");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Edit POST: Unexpected error while updating actor {actor.Id}.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the actor.");
                }
            }
            else
            {
                _logger.LogWarning($"Edit POST: Model state is invalid for actor {actor.Id}.");
            }

            // If we reach here, something failed; redisplay the form with validation errors
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete GET: No id provided.");
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null)
            {
                _logger.LogWarning($"Delete GET: Actor with id {id} not found.");
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Delete POST: Successfully deleted actor {id}.");
            }
            else
            {
                _logger.LogWarning($"Delete POST: Actor with id {id} not found.");
            }

            return RedirectToAction(nameof(Index));
        }

        // Method to retrieve actor photo
        public async Task<IActionResult> GetActorPhoto(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("GetActorPhoto: No id provided.");
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null || actor.Photo == null)
            {
                _logger.LogWarning($"GetActorPhoto: Actor with id {id} not found or has no photo.");
                return NotFound();
            }

            // Determine the MIME type based on image format
            // Here, assuming JPEG. Adjust if necessary.
            return File(actor.Photo, "image/jpeg");
        }

        // POST: Actor/GenerateAITweets/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAITweets(int id)
        {
            var actor = await _context.Actors
                .Include(a => a.ActorTweets)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                _logger.LogWarning($"GenerateAITweets POST: Actor with id {id} not found.");
                return NotFound();
            }

            var prompt = $@"
Generate 20 short tweets about the actor '{actor.Name}'. 
Include a variety of sentiments: positive, negative, and neutral. 
Ensure tweets resemble actual tweets, with some having hashtags and mentions.
Output the tweets as a JSON array where each element is an object with 'tweet' and 'sentiment' fields.
Do not include any text outside the JSON array.";

            try
            {
                // Corrected Type Parameter: Use ActorTweet instead of List<ActorTweet>
                var aiTweets = await _aiService.GetChatGPTResponseAsync<ActorTweet>(prompt);

                if (aiTweets == null || !aiTweets.Any())
                {
                    TempData["AIError"] = "AI did not return any tweets.";
                    _logger.LogWarning($"GenerateAITweets POST: AI did not return any tweets for actor {id}.");
                    return RedirectToAction("Details", new { id = actor.Id });
                }

                // Optional: Clear existing tweets before adding new ones
                _context.ActorTweets.RemoveRange(actor.ActorTweets);

                foreach (var tweet in aiTweets)
                {
                    tweet.ActorId = actor.Id; // Associate tweet with the actor
                    _context.ActorTweets.Add(tweet);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"GenerateAITweets POST: Successfully generated and saved AI tweets for actor {id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GenerateAITweets POST: Error generating AI tweets for actor {id}.");
                TempData["AIError"] = "Error generating AI tweets: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = actor.Id });
        }

        // Helper function to calculate overall sentiment
        private string CalculateOverallSentiment(List<ActorTweet> tweets)
        {
            var sentimentCounts = tweets
                .GroupBy(r => r.Sentiment)
                .ToDictionary(g => g.Key, g => g.Count());

            return sentimentCounts
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault().Key ?? "No Sentiment";
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }

    }
}
