using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Extensions;
using RE_Waste.Models;
using RE_Waste.Models.Enums;
using RE_Waste.Services;
using System.Text.RegularExpressions;

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
        // Stores the services used by the wizard.
        private readonly ApplicationDbContext _context;
        private readonly IPostcodeService _postcodes;

        public WizardController(
            ApplicationDbContext context,
            IPostcodeService postcodes)
        {
            _context = context;       // Save the database context.
            _postcodes = postcodes;   // Save the postcode lookup service.
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
        // HELPER: MODEL OPTIONS FOR A BRAND
        // Newest models first, then "Other / not listed" last.
        // ====================================================================
        private async Task<List<string>> ModelOptionsFor(string? brand)
        {
            var names = await _context.PhoneModels
                .Where(p => p.IsActive && p.Brand == brand)
                .OrderByDescending(p => p.ReleaseYear)
                .ThenBy(p => p.Name)
                .Select(p => p.Name)
                .ToListAsync();

            names.Add(PhoneModel.Other);
            return names;
        }

        // ====================================================================
        // HELPER: RELEASE YEAR
        // The year a listed model came out, or null when the brand or model
        // was typed in ("Other") or the list doesn't know it.
        // ====================================================================
        private async Task<int?> ReleaseYearOf(string? brand, string? model)
        {
            if (!PhoneBrands.IsListed(brand) || string.IsNullOrEmpty(model))
                return null;

            return await _context.PhoneModels
                .Where(p => p.IsActive && p.Brand == brand && p.Name == model)
                .Select(p => p.ReleaseYear)
                .FirstOrDefaultAsync();
        }

        // ====================================================================
        // HELPER: STEP VIEW DATA
        // Fills the values Step.cshtml needs. Used by Step, and by Next when
        // an answer is refused and the same question is shown again.
        // ====================================================================
        private async Task SetStepViewData(WizardState state, List<Question> questions, Question question, string currentAnswer)
        {
            ViewBag.StepNumber = state.Step + 1; // Display steps starting at 1.
            ViewBag.TotalSteps = questions.Count; // Pass the total number of questions.
            ViewBag.IsLast = state.Step == questions.Count - 1; // Check for the final question.
            ViewBag.CurrentAnswer = currentAnswer; // The answer to show in the input.

            state.Answers.TryGetValue(DeviceField.Brand, out var brand);
            state.Answers.TryGetValue(DeviceField.Model, out var model);

            // Brand: a brand typed in on the Model step shows as "Other" in the drop-down.
            if (question.DeviceField == DeviceField.Brand
                && currentAnswer != "" && !PhoneBrands.All.Contains(currentAnswer))
            {
                ViewBag.CurrentAnswer = PhoneBrands.Other;
            }

            // Model: a searchable list for a listed brand, or brand and model boxes for "Other".
            if (question.DeviceField == DeviceField.Model)
            {
                var options = await ModelOptionsFor(brand);
                var listed = PhoneBrands.IsListed(brand);

                ViewBag.BrandListed = listed;
                ViewBag.ModelOptions = options;
                ViewBag.OtherBrand = listed || brand == PhoneBrands.Other ? "" : brand ?? "";
                ViewBag.OtherModel = "";

                // A model that isn't in the list was typed in: select "Other" and show it in the box.
                if (currentAnswer != "" && !options.Contains(currentAnswer))
                {
                    ViewBag.CurrentAnswer = PhoneModel.Other;
                    ViewBag.OtherModel = currentAnswer;
                }
            }

            // Age: estimate it from the model's release year, when the model is from the list.
            if (question.DeviceField == DeviceField.AgeInYears)
            {
                var year = await ReleaseYearOf(brand, model);

                if (year != null)
                {
                    ViewBag.ReleaseYear = year;
                    ViewBag.PhoneName = $"{brand} {model}";
                    ViewBag.EstimatedAge = Math.Clamp(
                        DateTime.Today.Year - year.Value,
                        AnswerValidator.MinNumber,
                        AnswerValidator.MaxNumber);
                }
            }
        }

        // ====================================================================
        // HELPER: CHECK MODEL STEP
        // The Model step can also carry a typed-in brand and model ("Other").
        // Returns the error and the name of the box it belongs to.
        // ====================================================================
        private async Task<(string? Error, string Field)> CheckModel(
            WizardState state, string? answer, string? otherBrand, string? otherModel)
        {
            state.Answers.TryGetValue(DeviceField.Brand, out var brand);

            // Brand was "Other": both the brand and the model are typed in.
            if (!PhoneBrands.IsListed(brand))
            {
                var brandError = AnswerValidator.ValidateText(
                    otherBrand, AnswerValidator.MaxLength(DeviceField.Brand), AnswerValidator.TypeBrandMessage);

                if (brandError != null)
                    return (brandError, "otherBrand");

                return (AnswerValidator.ValidateText(
                    otherModel, AnswerValidator.MaxLength(DeviceField.Model), AnswerValidator.TypeModelMessage), "otherModel");
            }

            // Listed brand: the answer must be one of its models, or "Other / not listed".
            var value = (answer ?? string.Empty).Trim();

            if (value.Length == 0)
                return (AnswerValidator.RequiredMessage, "answer");

            if (!(await ModelOptionsFor(brand)).Contains(value))
                return (AnswerValidator.ChoiceMessage, "answer");

            // "Other / not listed": the model is typed into the extra box.
            if (value == PhoneModel.Other)
                return (AnswerValidator.ValidateText(
                    otherModel, AnswerValidator.MaxLength(DeviceField.Model), AnswerValidator.TypeModelMessage), "otherModel");

            return (null, "answer");
        }

        // ====================================================================
        // HELPER: SAVE BRAND
        // A different brand clears the model and age, because they belonged
        // to the old brand. Picking "Other" again keeps a typed-in brand.
        // ====================================================================
        private static void SaveBrand(WizardState state, string brand)
        {
            state.Answers.TryGetValue(DeviceField.Brand, out var previous);

            var typedBefore = !string.IsNullOrEmpty(previous) && !PhoneBrands.All.Contains(previous);

            if (brand == PhoneBrands.Other && typedBefore)
                return;

            if (brand != previous)
            {
                state.Answers.Remove(DeviceField.Model);
                state.Answers.Remove(DeviceField.AgeInYears);
            }

            state.Answers[DeviceField.Brand] = brand;
        }

        // ====================================================================
        // HELPER: SAVE MODEL
        // Typed-in values replace "Other" and "Other / not listed", so later
        // steps get the real brand and model. A new model clears the age.
        // ====================================================================
        private static void SaveModel(WizardState state, string? answer, string? otherBrand, string? otherModel)
        {
            state.Answers.TryGetValue(DeviceField.Brand, out var brand);
            state.Answers.TryGetValue(DeviceField.Model, out var previous);

            string model;

            if (PhoneBrands.IsListed(brand))
            {
                var chosen = answer!.Trim();
                model = chosen == PhoneModel.Other ? otherModel!.Trim() : chosen;
            }
            else
            {
                state.Answers[DeviceField.Brand] = otherBrand!.Trim();
                model = otherModel!.Trim();
            }

            if (model != previous)
                state.Answers.Remove(DeviceField.AgeInYears);

            state.Answers[DeviceField.Model] = model;
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
                return RedirectToAction(nameof(Postcode)); // Finish if past the last step.

            var question = questions[state.Step]; // Get the current question.

            // Restore the saved answer, if one exists.
            var savedAnswer = state.Answers.TryGetValue(question.DeviceField, out var answer)
                ? answer
                : "";

            await SetStepViewData(state, questions, question, savedAnswer);

            return View(question); // Send the question to Step.cshtml.
        }



        // ====================================================================
        // ACTION: NEXT (Save answer and move forward)
        // POST is required here to modify data. PRG pattern prevents resubmits.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Next(string? answer, string? otherBrand, string? otherModel)
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
                return RedirectToAction(nameof(Postcode)); // Avoid going past the final step.

            var question = questions[state.Step]; // Get the question being answered.

            // W5: check the answer before saving it. On an error, show the same step again
            // (not a redirect) so the message and the typed answer both stay on the page.
            string? error;
            var errorField = "answer";

            if (question.DeviceField == DeviceField.Model)
            {
                (error, errorField) = await CheckModel(state, answer, otherBrand, otherModel);
            }
            else
            {
                error = AnswerValidator.Validate(question, answer);

                // The brand must be one of the drop-down options.
                if (error == null && question.DeviceField == DeviceField.Brand
                    && !PhoneBrands.All.Contains(answer!.Trim()))
                {
                    error = AnswerValidator.ChoiceMessage;
                }
            }

            if (error != null)
            {
                await SetStepViewData(state, questions, question, answer ?? string.Empty);

                // Show the typed brand and model again.
                if (otherBrand != null)
                    ViewBag.OtherBrand = otherBrand;
                if (otherModel != null)
                    ViewBag.OtherModel = otherModel;

                ViewBag.ErrorMessage = error;
                ViewBag.ErrorField = errorField; // Which box to mark in red.
                return View(nameof(Step), question);
            }

            // Save the trimmed answer using DeviceField as the dictionary key.
            if (question.DeviceField == DeviceField.Brand)
                SaveBrand(state, answer!.Trim());
            else if (question.DeviceField == DeviceField.Model)
                SaveModel(state, answer, otherBrand, otherModel);
            else
                state.Answers[question.DeviceField] = answer!.Trim();

            state.Step++; // Move to the next question.

            HttpContext.Session.SetWizardState(state); // Save progress to the session.

            // Go to Complete after the final question.
            return state.Step >= questions.Count
                ? RedirectToAction(nameof(Postcode))
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

        // ================================================================
        // ACTIONS: POSTCODE
        // Display the postcode form and validate the submitted postcode.
        // ================================================================
        // GET: Show the form with any previously saved postcode.
        [HttpGet]
        public IActionResult Postcode()
        {
            var state = HttpContext.Session.GetWizardState();

            // Start again if the session has expired.
            if (state == null)
                return RedirectToAction(nameof(Start));

            // Fill the form with the saved postcode, if one exists.
            return View("Postcode", state.Postcode);
        }

        // POST: Validate and save the user's postcode.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Postcode(string postcode)
        {
            var state = HttpContext.Session.GetWizardState();

            // Start again if the session has expired.
            if (state == null)
                return RedirectToAction(nameof(Start));

            // keep origional input so it can be shown again on errors
            var originalPostcode = postcode ?? string.Empty;

            // remove spaces and convert to uppercase
            postcode = originalPostcode.Trim().ToUpperInvariant();

            // Reject an empty postcode.
            if (string.IsNullOrWhiteSpace(postcode))
            {
                ViewBag.ErrorMessage = "Please enter your postcode.";
                return View("Postcode", originalPostcode);
            }

            // Check the postcode's format before looking it up.
            var pattern = @"^[A-Z]{1,2}[0-9][A-Z0-9]? ?[0-9][A-Z]{2}$";

            if (!Regex.IsMatch(postcode, pattern))
            {
                ViewBag.ErrorMessage = "Please enter a valid postcode.";
                return View("Postcode", originalPostcode);
            }


            // Check whether the postcode can be found by the service.
            try
            {
                var result = await _postcodes.LookupAsync(postcode);

                // Show an error if the postcode cannot be found.
                if (result == null)
                {
                    ViewBag.ErrorMessage =
                        "We couldn't find that postcode. Please check it and try again.";

                    return View("Postcode", originalPostcode);
                }
            }
            catch
            {
                // Keep the user's input if the lookup fails.
                ViewBag.ErrorMessage =
                    "We couldn't find that postcode. Please check it and try again.";

                return View("Postcode", originalPostcode);
            }

            // Store only the postcode text, not location coordinates.
            state.Postcode = postcode;
            HttpContext.Session.SetWizardState(state);

            // Continue to the results/review page.
            return RedirectToAction(nameof(Complete));

        }
    }
}