using Microsoft.AspNetCore.Mvc;
using RE_Waste.Extensions;
using RE_Waste.Models;
using RE_Waste.Models.Enums;
using System.Numerics;

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
        [HttpGet]
        public IActionResult Step()
        {
            var state = HttpContext.Session.GetWizardState();

            // If no state exists (session expired or user typed the URL manually)
            if (state == null)
                return RedirectToAction(nameof(Start));

            // If they somehow passed the last step, send them to the Complete page
            if (state.Step < 0) state.Step = 0;
            if (state.Step >= Steps.Length)
                return RedirectToAction(nameof(Complete));

            var currentField = Steps[state.Step];

            // Pass data to the View so it knows what to display
            ViewBag.StepNumber = state.Step +1;
            ViewBag.TotalSteps = Steps.Length;

            // Temporary Value for testing purposes - Passes the current question to the View so it can display the right prompt
            ViewBag.CurrentField = currentField;


            // Check if they already answered this question (e.g. if they clicked Back)
            ViewBag.CurrentAnswer = state.Answers.TryGetValue(currentField, out var a) ? a : "";
            ViewBag.IsLast = state.Step == Steps.Length - 1;

            return View(); // renders views/wizard/step.cshtml
        }

        // ====================================================================
        // ACTION: NEXT (Save answer and move forward)
        // POST is required here to modify data. PRG pattern prevents resubmits.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Next(string answer)
        {
            var state = HttpContext.Session.GetWizardState();
            if (state == null)
                return RedirectToAction(nameof(Start));

            //guard against stale step value beyond step list
            if (state.Step < 0) state.Step = 0;
            if (state.Step >= Steps.Length)
                return RedirectToAction(nameof(Complete));

            // 1. Save the answer (or empty string if they clicked Next without typing)
            var currentField = Steps[state.Step];
            state.Answers[currentField] = answer ?? string.Empty;
            // 2. Move to the next step
            state.Step++;

            // 3. Save the updated state back to the session
            HttpContext.Session.SetWizardState(state);

            // 4. POST-REDIRECT-GET: Redirect so refreshing doesn't re-submit the form
            return state.Step >= Steps.Length
                ? RedirectToAction(nameof(Complete)) // go to end page
                : RedirectToAction(nameof(Step)); // go to next question
        }

        // ====================================================================
        // ACTION: BACK (Move backward)
        // Does NOT delete answers. Only moves the Step counter down.
        // ====================================================================
        [HttpGet]
        public IActionResult Back()
        {
            var state = HttpContext.Session.GetWizardState();
            if (state == null)
                return RedirectToAction(nameof(Start));

            if (state.Step > 0)
                state.Step--; // never below 0 - answers untouched

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
