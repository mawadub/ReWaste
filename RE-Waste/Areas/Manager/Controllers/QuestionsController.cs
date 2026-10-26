
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager)]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Manager/Questions
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .AsNoTracking()
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();

            return View(questions);
        }

        // GET: Manager/Questions/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Question());
        }

        // POST: Manager/Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            // Choice questions must first be created inactive so options
            // can be added before the question is activated.
            question.IsActive = question.AnswerType != AnswerType.Choice;

            await ValidateQuestionRules(question);

            if (!ModelState.IsValid)
                return View(question);

            var maxOrder = await _context.Questions
                .Select(q => (int?)q.DisplayOrder)
                .MaxAsync() ?? 0;

            question.DisplayOrder = maxOrder + 1;

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Manager/Questions/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            question.Options = question.Options
                .OrderBy(o => o.DisplayOrder)
                .ToList();

            return View(question);
        }

        // POST: Manager/Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question question)
        {
            if (id != question.Id)
                return BadRequest();

            var existing = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existing == null)
                return NotFound();

            await ValidateQuestionRules(question, id);

            if (!ModelState.IsValid)
            {
                question.Options = await _context.AnswerOptions
                    .Where(o => o.QuestionId == id)
                    .OrderBy(o => o.DisplayOrder)
                    .ToListAsync();

                return View(question);
            }

            existing.Text = question.Text;
            existing.AnswerType = question.AnswerType;
            existing.DeviceField = question.DeviceField;
            existing.IsActive = question.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Manager/Questions/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            var neighbour = await _context.Questions
                .Where(q => q.DisplayOrder < question.DisplayOrder)
                .OrderByDescending(q => q.DisplayOrder)
                .FirstOrDefaultAsync();

            if (neighbour != null)
            {
                var order = question.DisplayOrder;
                question.DisplayOrder = neighbour.DisplayOrder;
                neighbour.DisplayOrder = order;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Manager/Questions/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            var neighbour = await _context.Questions
                .Where(q => q.DisplayOrder > question.DisplayOrder)
                .OrderBy(q => q.DisplayOrder)
                .FirstOrDefaultAsync();

            if (neighbour != null)
            {
                var order = question.DisplayOrder;
                question.DisplayOrder = neighbour.DisplayOrder;
                neighbour.DisplayOrder = order;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Manager/Questions/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            if (!question.IsActive)
            {
                var duplicate = await _context.Questions.AnyAsync(q =>
                    q.Id != id &&
                    q.IsActive &&
                    q.DeviceField == question.DeviceField);

                if (duplicate)
                {
                    TempData["QuestionError"] =
                        $"Another active question already uses {question.DeviceField}.";

                    return RedirectToAction(nameof(Index));
                }

                if (question.AnswerType == AnswerType.Choice)
                {
                    var hasActiveOption = await _context.AnswerOptions
                        .AnyAsync(o =>
                            o.QuestionId == id &&
                            o.IsActive);

                    if (!hasActiveOption)
                    {
                        TempData["QuestionError"] =
                            "Add at least one active option before activating this Choice question.";

                        return RedirectToAction(nameof(Edit), new { id });
                    }
                }
            }

            question.IsActive = !question.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Manager/Questions/CreateOption?questionId=5
        [HttpGet]
        public async Task<IActionResult> CreateOption(int questionId)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return NotFound();

            if (question.AnswerType != AnswerType.Choice)
                return BadRequest();

            var maxOrder = await _context.AnswerOptions
                .Where(o => o.QuestionId == questionId)
                .Select(o => (int?)o.DisplayOrder)
                .MaxAsync() ?? 0;

            ViewBag.QuestionText = question.Text;

            return View(new AnswerOption
            {
                QuestionId = questionId,
                DisplayOrder = maxOrder + 1,
                IsActive = true
            });
        }

        // POST: Manager/Questions/CreateOption
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOption(
            int questionId,
            AnswerOption option)
        {
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return NotFound();

            if (question.AnswerType != AnswerType.Choice)
                return BadRequest();

            ModelState.Remove(nameof(option.QuestionId));
            ModelState.Remove(nameof(option.IsActive));
            ModelState.Remove(nameof(option.Question));

            ValidateOptionValue(question.DeviceField, option.Value);

            if (!ModelState.IsValid)
            {
                ViewBag.QuestionText = question.Text;
                option.QuestionId = questionId;

                return View(option);
            }

            option.QuestionId = questionId;
            option.IsActive = true;

            _context.AnswerOptions.Add(option);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit),
                new { id = questionId });
        }

        // GET: Manager/Questions/EditOption/5
        [HttpGet]
        public async Task<IActionResult> EditOption(int id)
        {
            var option = await _context.AnswerOptions
                .Include(o => o.Question)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (option == null)
                return NotFound();

            if (option.Question.AnswerType != AnswerType.Choice)
                return BadRequest();

            ViewBag.QuestionText = option.Question.Text;

            return View(option);
        }

        // POST: Manager/Questions/EditOption/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOption(
            int id,
            AnswerOption option)
        {
            if (id != option.Id)
                return BadRequest();

            var existing = await _context.AnswerOptions
                .Include(o => o.Question)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existing == null)
                return NotFound();

            if (existing.Question.AnswerType != AnswerType.Choice)
                return BadRequest();

            ModelState.Remove(nameof(option.QuestionId));
            ModelState.Remove(nameof(option.Question));
            ModelState.Remove(nameof(option.IsActive));

            ValidateOptionValue(
                existing.Question.DeviceField,
                option.Value);

            if (!ModelState.IsValid)
            {
                ViewBag.QuestionText = existing.Question.Text;
                option.QuestionId = existing.QuestionId;
                option.IsActive = existing.IsActive;

                return View(option);
            }

            existing.Text = option.Text;
            existing.Value = option.Value;
            existing.DisplayOrder = option.DisplayOrder;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit),
                new { id = existing.QuestionId });
        }

        // POST: Manager/Questions/ToggleOptionActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleOptionActive(int id)
        {
            var option = await _context.AnswerOptions
                .Include(o => o.Question)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (option == null)
                return NotFound();

            var question = option.Question;

            if (question.AnswerType != AnswerType.Choice)
                return BadRequest();

            if (option.IsActive &&
                question.IsActive)
            {
                var anotherActiveOption = await _context.AnswerOptions
                    .AnyAsync(o =>
                        o.QuestionId == question.Id &&
                        o.Id != option.Id &&
                        o.IsActive);

                if (!anotherActiveOption)
                {
                    TempData["QuestionError"] =
                        "An active Choice question must have at least one active option.";

                    return RedirectToAction(nameof(Edit),
                        new { id = question.Id });
                }
            }

            option.IsActive = !option.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit),
                new { id = question.Id });
        }

        // Validates required answer types and active DeviceField uniqueness.
        private async Task ValidateQuestionRules(
            Question question,
            int? excludeQuestionId = null)
        {
            if (question.IsActive)
            {
                var duplicate = await _context.Questions.AnyAsync(q =>
                    q.IsActive &&
                    q.DeviceField == question.DeviceField &&
                    (!excludeQuestionId.HasValue ||
                     q.Id != excludeQuestionId.Value));

                if (duplicate)
                {
                    ModelState.AddModelError(
                        nameof(question.DeviceField),
                        "Another active question already uses this device field.");
                }
            }

            AnswerType? requiredType = question.DeviceField switch
            {
                DeviceField.AgeInYears => AnswerType.Number,

                DeviceField.Condition => AnswerType.Choice,
                DeviceField.HasPersonalData => AnswerType.Choice,
                DeviceField.HasBatteryDamage => AnswerType.Choice,

                DeviceField.Brand => AnswerType.Text,
                DeviceField.Model => AnswerType.Text,

                _ => null
            };

            if (requiredType.HasValue &&
                question.AnswerType != requiredType.Value)
            {
                ModelState.AddModelError(
                    nameof(question.AnswerType),
                    $"{question.DeviceField} must use the {requiredType.Value} answer type.");
            }

            if (question.IsActive &&
                question.AnswerType == AnswerType.Choice)
            {
                var hasActiveOption = question.Id > 0 &&
                    await _context.AnswerOptions.AnyAsync(o =>
                        o.QuestionId == question.Id &&
                        o.IsActive);

                if (!hasActiveOption)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "An active Choice question must have at least one active option.");
                }
            }
        }

        // Validates option values based on the question's DeviceField.
        private void ValidateOptionValue(
            DeviceField field,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(
                    nameof(AnswerOption.Value),
                    "An option value is required.");

                return;
            }

            bool valid = field switch
            {
                DeviceField.Condition =>
                    value is "Working"
                        or "PartlyWorking"
                        or "NotWorking"
                        or "Damaged",

                DeviceField.HasPersonalData or DeviceField.HasBatteryDamage =>
                    value is "True" or "False",

                _ => true
            };

            if (!valid)
            {
                var message = field switch
                {
                    DeviceField.Condition =>
                        "Condition values must be Working, PartlyWorking, NotWorking, or Damaged.",

                    DeviceField.HasPersonalData or DeviceField.HasBatteryDamage =>
                        "Values for this field must be True or False.",

                    _ => "Invalid option value."
                };

                ModelState.AddModelError(
                    nameof(AnswerOption.Value),
                    message);
            }
        }
    }
}
