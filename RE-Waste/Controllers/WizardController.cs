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
        // DATABASE CONTEXT
        // ====================================================================
        // Stores the database context for loading questions.
        private readonly ApplicationDbContext _context;

        public WizardController(ApplicationDbContext context)
        {
            _context = context; // Save the injected database context.
        }

        // ====================================================================
        // HELPER: LOAD ACTIVE QUESTIONS
        // ====================================================================
        private async Task<List<Question>> LoadQuestions()
        {
            return await _context.Questions
                .Where(q => q.IsActive) // Only include active questions.
                .OrderBy(q => q.DisplayOrder) // Show questions in the correct order.
                .Include(q => q.Options
                    .Where(o => o.IsActive) // Only include active options.
                    .OrderBy(o => o.DisplayOrder)) // Order the answer options.
                .ToListAsync();
        }

        // ====================================================================
        // ACTION: STEP
        // Loads and displays the current question.
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Step()
        {
            var state = HttpContext.Session.GetWizardState(); // Get saved progress.

            if (state == null)
                return RedirectToAction(nameof(Start)); // Restart if the session expired.

            var questions = await LoadQuestions(); // Get the current active questions.

            if (questions.Count == 0)
                return View("NoQuestions"); // Handle an empty question list.

            if (state.Step < 0)
                state.Step = 0; // Prevent a negative step number.

            if (state.Step >= questions.Count)
                return RedirectToAction(nameof(Complete)); // Finish if past the last step.

            var question = questions[state.Step]; // Get the current question.

            ViewBag.StepNumber = state.Step + 1; // Display steps starting at 1.
            ViewBag.TotalSteps = questions.Count; // Pass the total number of questions.
            ViewBag.IsLast = state.Step == questions.Count - 1; // Check for the final question.

            // Restore the saved answer, if one exists.
            ViewBag.CurrentAnswer =
                state.Answers.TryGetValue(question.DeviceField, out var answer)
                    ? answer
                    : "";

            return View(question); // Send the question to Step.cshtml.
        }



        // ====================================================================
        // ACTION: NEXT (Save answer and move forward)
        // POST is required here to modify data. PRG pattern prevents resubmits.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Next(string answer)
        {
            var state = HttpContext.Session.GetWizardState(); // Get saved progress.

            if (state == null)
                return RedirectToAction(nameof(Start)); // Restart if the session expired.

            var questions = await LoadQuestions(); // Get the current active questions.

            if (questions.Count == 0) 
                return View("NoQuestions"); // Handle an empty question list.

            if (state.Step < 0)
                state.Step = 0; // Prevent a negative step number.

            if (state.Step >= questions.Count)
                return RedirectToAction(nameof(Complete)); // Avoid going past the final step.

            var question = questions[state.Step]; // Get the question being answered.

            // Save the answer using DeviceField as the dictionary key.
            state.Answers[question.DeviceField] = answer ?? string.Empty;

            state.Step++; // Move to the next question.

            HttpContext.Session.SetWizardState(state); // Save progress to the session.

            // Go to Complete after the final question.
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
            var state = HttpContext.Session.GetWizardState(); // Get saved progress.

            if (state == null)
                return RedirectToAction(nameof(Start)); // Restart if the session expired.

            var questions = await LoadQuestions(); // Get the current active questions.

            if (questions.Count == 0)
                return View("NoQuestions"); // Handle an empty question list.

            if (state.Step > 0)
                state.Step--; // Move back one step.

            if (state.Step >= questions.Count)
                state.Step = questions.Count - 1; // Keep the step within the valid range.

            HttpContext.Session.SetWizardState(state); // Save the updated step.

            return RedirectToAction(nameof(Step)); // Display the previous question.
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
