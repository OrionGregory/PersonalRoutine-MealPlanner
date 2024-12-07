// Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Assignment3.Data;

namespace Assignment3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);  // Get the current logged-in user
            if (user == null)
            {
                // Handle when user is not found
                return View(user);
            }

            var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == user.Id);  // Retrieve the Person record based on the user's ID
            if (person == null)
            {
                // Handle case when person record is not found
                return View(person);
            }

            // Pass the Person model to the view
            return View(person);
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        //// GET: Home/Error
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    var errorModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        //    return View(errorModel);
        //}
    }
}
