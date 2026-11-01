
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.ViewModels;

namespace RE_Waste.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager)]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // G5: the counts shown at the top of the dashboard.
        public async Task<IActionResult> Index()
        {
            var model = new ManagerDashboardViewModel
            {
                ActiveQuestions = await _context.Questions.CountAsync(q => q.IsActive),
                ActiveRules = await _context.RecommendationRules.CountAsync(r => r.IsActive),
                DropOffPoints = await _context.DropOffPoints.CountAsync(),
                // Only the total. Managers must never see saved results or who owns them.
                SavedResults = await _context.SavedResponses.CountAsync()
            };

            return View(model);
        }
    }
}
