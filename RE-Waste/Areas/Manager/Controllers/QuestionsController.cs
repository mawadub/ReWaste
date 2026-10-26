
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
            if (!ModelState.IsValid)
                return View(question);

            var maxOrder = await _context.Questions
                .Select(q => (int?)q.DisplayOrder)
                .MaxAsync() ?? 0;

            question.DisplayOrder = maxOrder + 1;
            question.IsActive = true;

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

            if (!ModelState.IsValid)
            {
                question.Options = await _context.AnswerOptions
                    .Where(o => o.QuestionId == id)
                    .OrderBy(o => o.DisplayOrder)
                    .ToListAsync();

                return View(question);
            }

            var existing = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existing == null)
                return NotFound();

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

            // These values are assigned by the server.
            ModelState.Remove(nameof(option.QuestionId));
            ModelState.Remove(nameof(option.IsActive));
            ModelState.Remove(nameof(option.Question));

            if (!ModelState.IsValid)
            {
                ViewBag.QuestionText = question.Text;
                option.QuestionId = questionId;
                return View(option);
            }

            option.QuestionId = questionId;
            option.IsActive = true;

            // Respect the DisplayOrder submitted by the manager.
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

            // Question relationship and active status aren't editable here.
            ModelState.Remove(nameof(option.QuestionId));
            ModelState.Remove(nameof(option.Question));
            ModelState.Remove(nameof(option.IsActive));

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

            if (option.Question.AnswerType != AnswerType.Choice)
                return BadRequest();

            option.IsActive = !option.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit),
                new { id = option.QuestionId });
        }
    }
}
