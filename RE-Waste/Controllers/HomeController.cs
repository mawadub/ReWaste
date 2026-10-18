using Microsoft.AspNetCore.Mvc;
using RE_Waste.Models;
using RE_Waste.Services;
using System.Diagnostics;

namespace RE_Waste.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecommendationService _recommendationService;
        public HomeController(
            ILogger<HomeController> logger,
            IRecommendationService service)
        {
            _logger = logger;
            _recommendationService = service;
        }

        // Temporary action for testing the recommendation service
        [HttpGet]
        public async Task<IActionResult> TestRecommendation(Device device)
        {

            var recommendations =
                await _recommendationService.RecommendAsync(device);

            return Json(recommendations);
        }

        /*
         * below is the original code for the constructor above is the version used to test the RecommendAsync
         public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

         */


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
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
