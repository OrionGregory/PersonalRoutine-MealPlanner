// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;
using System.Threading.Tasks;

namespace Assignment3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly WeatherService _weatherService;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context, WeatherService weatherService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _weatherService = weatherService;
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
                return RedirectToAction("Create", "Person");
            }

            var completedExercises = await _context.CompletedExercises
                .Where(ce => ce.UserId == userId && ce.CompletedDate.Date == DateTime.Today)
                .Select(ce => ce.ExerciseId)
                .ToListAsync();

            ViewBag.CompletedExercises = completedExercises;

            return View(person);
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTemperature(string zipCode)
        {
            var (temperature, city) = await _weatherService.GetTemperatureAsync(zipCode);
            return Json(new { temperature, city });
        }
    }
}
