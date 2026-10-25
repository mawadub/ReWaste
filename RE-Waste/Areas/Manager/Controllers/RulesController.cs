using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace RE_Waste.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager)]
    public class RulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rules = await _context.RecommendationRules
                .OrderByDescending(r => r.Priority)
                .ToListAsync();

            return View(rules);

        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new RuleFormViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RuleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var rule = new RecommendationRule
            {
                Name = model.Name,
                Route = model.Route,
                Reason = model.Reason,
                Priority = model.Priority,
                Condition = model.Condition,
                MinAgeInYears = model.MinAgeInYears,
                MaxAgeInYears = model.MaxAgeInYears,
                HasPersonalData = model.HasPersonalData,
                HasBatteryDamage = model.HasBatteryDamage,
                IsActive = true
            };

            _context.RecommendationRules.Add(rule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rule = await _context.RecommendationRules.FindAsync(id);

            if (rule == null)
            {
                return NotFound();
            }

            var model = new RuleFormViewModel
            {
                Name = rule.Name,
                Route = rule.Route,
                Reason = rule.Reason,
                Priority = rule.Priority,
                Condition = rule.Condition,
                MinAgeInYears = rule.MinAgeInYears,
                MaxAgeInYears = rule.MaxAgeInYears,
                HasPersonalData = rule.HasPersonalData,
                HasBatteryDamage = rule.HasBatteryDamage,
                IsActive = rule.IsActive
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RuleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var rule = await _context.RecommendationRules.FindAsync(id);

            if (rule == null)
            {
                return NotFound();
            }

            rule.Name = model.Name;
            rule.Route = model.Route;
            rule.Reason = model.Reason;
            rule.Priority = model.Priority;
            rule.Condition = model.Condition;
            rule.MinAgeInYears = model.MinAgeInYears;
            rule.MaxAgeInYears = model.MaxAgeInYears;
            rule.HasPersonalData = model.HasPersonalData;
            rule.HasBatteryDamage = model.HasBatteryDamage;
            rule.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var rule = await _context.RecommendationRules.FindAsync(id);

            if (rule == null)
            {
                return NotFound();
            }

            rule.IsActive = !rule.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
