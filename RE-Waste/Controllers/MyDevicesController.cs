using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Services;
using RE_Waste.ViewModels;


namespace RE_Waste.Controllers
{   // Lists the logged-in user's Saved results 
    [Authorize]
    public class MyDevicesController:Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostcodeService _postcodes;
        private readonly IDropOffService _dropOff;
        public MyDevicesController(
            ApplicationDbContext context,
            IPostcodeService postcodes,
            IDropOffService dropOff)
        {
            _context = context;
            _postcodes = postcodes;
            _dropOff = dropOff;
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

        // U5: shows one saved result with its routes and the nearest places for each
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Owner check: the row must belong to the logged-in user, otherwise it "doesn't exist"
            var saved = await _context.SavedResponses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (saved == null)
                return NotFound();

            // Route1 is always set; Route2 and Route3 only when they were saved
            var routes = new List<SavedRouteDetails>
            {
                new() { Route = saved.Route1, Reason = saved.Reason1 }
            };

            if (saved.Route2 != null)
                routes.Add(new() { Route = saved.Route2.Value, Reason = saved.Reason2 ?? string.Empty });

            if (saved.Route3 != null)
                routes.Add(new() { Route = saved.Route3.Value, Reason = saved.Reason3 ?? string.Empty });

            // Look up the district once (e.g. "EH1"); the service returns null if it can't
            var geo = await _postcodes.LookupAsync(saved.PostcodeDistrict);

            if (geo != null)
            {
                foreach (var route in routes)
                    route.Places = await _dropOff.FindNearestAsync(route.Route, geo, 3);
            }

            return View(new SavedResponseDetailsViewModel
            {
                Saved = saved,
                Routes = routes,
                LocationFound = geo != null
            });
        }

        // U6: asks the user to confirm before deleting one of their saved results. Never deletes anything itself.
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Owner check: the row must belong to the logged-in user, otherwise it "doesn't exist"
            var saved = await _context.SavedResponses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (saved == null)
                return NotFound();

            return View(saved);
        }

        // U6: deletes the saved result after "Yes, delete". Only ever through POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Same owner check as Delete, so another user's id can't be deleted by posting it directly
            var saved = await _context.SavedResponses
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (saved == null)
                return NotFound();

            _context.SavedResponses.Remove(saved);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Saved result deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
