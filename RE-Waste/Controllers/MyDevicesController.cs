using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;

namespace RE_Waste.Controllers
{   // Lists the logged-in user's Saved results 
    [Authorize]
    public class MyDevicesController:Controller
    {
        private readonly ApplicationDbContext _context;
        public MyDevicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = await _context.SavedResponses
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(items);
        }

    }
}
