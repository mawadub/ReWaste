using Microsoft.AspNetCore.Mvc;
using RE_Waste.Extensions;
using RE_Waste.Models;
using RE_Waste.Models.Enums;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;


namespace RE_Waste.Controllers
{
    public class WizardController : Controller
    {
        // ====================================================================
        // HARD-CODED STEPS
        // Until W2 is ready, we hard-code the order of questions here.
        // ====================================================================
        private static readonly DeviceField[] Steps =
        {
            DeviceField.Brand,
            DeviceField.Model,
            DeviceField.AgeInYears,
            DeviceField.Condition,
            DeviceField.HasPersonalData,
            DeviceField.HasBatteryDamage
        };

        // ====================================================================
        // ACTION: START
        // Clears old data, makes a fresh state, and moves to Step 1.
        // ====================================================================
        [HttpGet]
        public IActionResult Start()
        {
            HttpContext.Session.ClearWizardState(); // Wipe old progress
            HttpContext.Session.SetWizardState(new WizardState()); // Create blank state
            return RedirectToAction(nameof(Step)); // Send user to the first question
        }

        // ====================================================================
        // ACTION: STEP (Show the question)
        // Loads state, checks for errors, and sends data to the View.
        // ====================================================================
        private readonly ApplicationDbContext _context;

        public WizardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<List<Question>> LoadQuestions()
        {
            return await _context.Questions
                .Include(q => q.Options)
                .Where(q => q.IsActive)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Step()
        {
            var state = HttpContext.Session.GetWizardState();

            if (state == null)
                return RedirectToAction(nameof(Start));

            var questions = await LoadQuestions();

            if (questions.Count == 0)
                return View("NoQuestions");

            if (state.Step < 0)
                state.Step = 0;

            if (state.Step >= questions.Count)
                return RedirectToAction(nameof(Complete));

            var question = questions[state.Step];

            ViewBag.StepNumber = state.Step + 1;
            ViewBag.TotalSteps = questions.Count;
            ViewBag.IsLast = state.Step == questions.Count - 1;

            ViewBag.CurrentAnswer =
                state.Answers.TryGetValue(question.DeviceField, out var answer)
                    ? answer
                    : "";

            return View(question);
        }



        // ====================================================================
        // ACTION: NEXT (Save answer and move forward)
        // POST is required here to modify data. PRG pattern prevents resubmits.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Next(string answer)
        {
            var state = HttpContext.Session.GetWizardState();

            if (state == null)
                return RedirectToAction(nameof(Start));

            var questions = await LoadQuestions();

            if (questions.Count == 0)
                return View("NoQuestions");

            if (state.Step < 0)
                state.Step = 0;

            if (state.Step >= questions.Count)
                return RedirectToAction(nameof(Complete));

            var question = questions[state.Step];

            state.Answers[question.DeviceField] = answer ?? string.Empty;

            state.Step++;

            HttpContext.Session.SetWizardState(state);

            return state.Step >= questions.Count
                ? RedirectToAction(nameof(Complete))
                : RedirectToAction(nameof(Step));
        }


        // ====================================================================
        // ACTION: BACK (Move backward)
        // Does NOT delete answers. Only moves the Step counter down.
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Back()
        {
            var state = HttpContext.Session.GetWizardState();

            if (state == null)
                return RedirectToAction(nameof(Start));

            var questions = await LoadQuestions();

            if (questions.Count == 0)
                return View("NoQuestions");

            if (state.Step > 0)
                state.Step--;

            if (state.Step >= questions.Count)
                state.Step = questions.Count - 1;

            HttpContext.Session.SetWizardState(state);

            return RedirectToAction(nameof(Step));
        }


        // ====================================================================
        // ACTION: RESTART
        // Wipes the session and starts over from scratch.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restart()
        {
            HttpContext.Session.ClearWizardState();
            return RedirectToAction(nameof(Start));
        }

        // ====================================================================
        // ACTION: COMPLETE (Temporary final page)
        // W6 will replace this. For now, it just shows all answers.
        // ====================================================================
        [HttpGet]
        public IActionResult Complete()
        {
            var state = HttpContext.Session.GetWizardState();
            if (state == null)
                return RedirectToAction(nameof(Start));

            return View(state); // passes the whole state to the view
        }


    }
}
