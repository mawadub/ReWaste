using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Services
{
    // W5: checks one wizard answer against its question.
    // Returns the message to show the user, or null if the answer is fine.
    // Static and with no database access, so it can be unit tested on its own (Week 7).
    public static class AnswerValidator
    {
        // The messages the user sees
        public const string RequiredMessage = "Please answer this question.";
        public const string NumberMessage = "Please enter a whole number between 0 and 30.";
        public const string ChoiceMessage = "Please choose one of the options.";

        // Number limits, also used by Step.cshtml so the browser and server checks always match
        public const int MinNumber = 0;
        public const int MaxNumber = 30;

        public static string? Validate(Question question, string? answer)
        {
            var value = (answer ?? string.Empty).Trim();

            // 1. Every answer is required; empty or spaces only is an error
            if (value.Length == 0)
                return RequiredMessage;

            switch (question.AnswerType)
            {
                // 2. Number: a whole number from 0 to 30. TryParse refuses "abc" and "2.5"
                case AnswerType.Number:
                    if (!int.TryParse(value, out var number) || number < MinNumber || number > MaxNumber)
                        return NumberMessage;
                    break;

                // 3. Text: Brand up to 50 characters, Model up to 100
                case AnswerType.Text:
                    var max = MaxLength(question.DeviceField);
                    if (value.Length > max)
                        return $"That answer is too long (maximum {max} characters).";
                    break;

                // 4. Choice: must equal the Value of one of the question's active options
                case AnswerType.Choice:
                    if (!question.Options.Any(o => o.IsActive && o.Value == value))
                        return ChoiceMessage;
                    break;
            }

            return null;
        }

        // Matches the column sizes in SavedResponse, so a valid answer can always be saved later
        public static int MaxLength(DeviceField field) => field switch
        {
            DeviceField.Brand => 50,
            _ => 100
        };
    }
}
