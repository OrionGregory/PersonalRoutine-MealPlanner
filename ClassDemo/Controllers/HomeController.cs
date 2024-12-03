// Controllers/HomeController.cs
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClassDemo.Data;
using ClassDemo.Models;
using ClassDemo.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var moviesCount = await _context.Movies.CountAsync();
            var actorsCount = await _context.Actors.CountAsync();

            var viewModel = new HomeIndexViewModel();

            // Fetch Random Movie
            if (moviesCount > 0)
            {
                var random = new Random();
                int movieSkip = random.Next(0, moviesCount);
                var randomMovie = await _context.Movies
                    .Include(m => m.MovieActors)
                        .ThenInclude(ma => ma.Actor)
                    .Include(m => m.AIReviews)
                    .Skip(movieSkip)
                    .FirstOrDefaultAsync();

                viewModel.RandomMovie = randomMovie;
            }
            else
            {
                _logger.LogWarning("Home Index: No movies found in the database.");
            }

            // Fetch Random Actor
            if (actorsCount > 0)
            {
                var random = new Random();
                int actorSkip = random.Next(0, actorsCount);
                var randomActor = await _context.Actors
                    .Include(a => a.MovieActors)
                        .ThenInclude(ma => ma.Movie)
                    .Include(a => a.ActorTweets)
                    .Skip(actorSkip)
                    .FirstOrDefaultAsync();

                viewModel.RandomActor = randomActor;
            }
            else
            {
                _logger.LogWarning("Home Index: No actors found in the database.");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
