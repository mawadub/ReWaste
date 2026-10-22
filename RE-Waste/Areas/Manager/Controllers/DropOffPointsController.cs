
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.Services;

namespace RE_Waste.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager)]
    public class DropOffPointsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostcodeService _postcodeService;

        public DropOffPointsController(
            ApplicationDbContext context,
            IPostcodeService postcodeService)
        {
            _context = context;
            _postcodeService = postcodeService;
        }

        // GET: Manager/DropOffPoints
        public async Task<IActionResult> Index()
        {
            var dropOffPoints = await _context.DropOffPoints
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(dropOffPoints);
        }

        // GET: Manager/DropOffPoints/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new DropOffPoint());
        }

        // POST: Manager/DropOffPoints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DropOffPoint model)
        {
            model.Postcode = (model.Postcode ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (!ModelState.IsValid)
                return View(model);

            var postcodeResult = await _postcodeService
                .LookupAsync(model.Postcode);

            if (postcodeResult == null)
            {
                ModelState.AddModelError(
                    nameof(model.Postcode),
                    "We couldn't find that postcode.");

                return View(model);
            }

            var point = new DropOffPoint
            {
                Name = model.Name,
                Address = model.Address,
                Postcode = model.Postcode,
                PointType = model.PointType,
                OpeningHours = model.OpeningHours,
                Website = model.Website,
                Latitude = postcodeResult.Latitude,
                Longitude = postcodeResult.Longitude
            };

            _context.DropOffPoints.Add(point);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Manager/DropOffPoints/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var point = await _context.DropOffPoints.FindAsync(id);

            if (point == null)
                return NotFound();

            return View(point);
        }

        // POST: Manager/DropOffPoints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DropOffPoint model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var point = await _context.DropOffPoints.FindAsync(id);

            if (point == null)
                return NotFound();

            model.Postcode = (model.Postcode ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            // Only look up the postcode if it has changed.
            if (!string.Equals(
                    model.Postcode,
                    (point.Postcode ?? string.Empty).Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                var postcodeResult = await _postcodeService
                    .LookupAsync(model.Postcode);

                if (postcodeResult == null)
                {
                    ModelState.AddModelError(
                        nameof(model.Postcode),
                        "We couldn't find that postcode.");

                    return View(model);
                }

                point.Latitude = postcodeResult.Latitude;
                point.Longitude = postcodeResult.Longitude;
            }

            point.Name = model.Name;
            point.Address = model.Address;
            point.Postcode = model.Postcode;
            point.PointType = model.PointType;
            point.OpeningHours = model.OpeningHours;
            point.Website = model.Website;
            point.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
