using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Services
{
    // W7: turns the wizard answers into a Device for the recommendation engine.
    // Returns null if any answer is missing or can't be read, so the caller can send the
    // user back to finish the wizard instead of guessing a value.
    // Static and with no database access, so it can be unit tested on its own (Week 7).
    public static class DeviceBuilder
    {
        public static Device? FromAnswers(WizardState state)
        {
            var answers = state.Answers;

            // Brand and Model: the trimmed text. Empty or spaces only counts as missing.
            if (!TryGetText(answers, DeviceField.Brand, out var brand)
                || !TryGetText(answers, DeviceField.Model, out var model))
            {
                return null;
            }

            // AgeInYears: a whole number. TryParse refuses "abc" instead of crashing.
            if (!TryGetText(answers, DeviceField.AgeInYears, out var ageText)
                || !int.TryParse(ageText, out var age))
            {
                return null;
            }

            // Condition: stored as the enum name, e.g. "PartlyWorking".
            // IsDefined also refuses numbers such as "7", which TryParse would otherwise accept.
            if (!TryGetText(answers, DeviceField.Condition, out var conditionText)
                || !Enum.TryParse<Condition>(conditionText, out var condition)
                || !Enum.IsDefined(condition))
            {
                return null;
            }

            // HasPersonalData and HasBatteryDamage: stored as "True" or "False".
            if (!TryGetText(answers, DeviceField.HasPersonalData, out var personalDataText)
                || !bool.TryParse(personalDataText, out var hasPersonalData))
            {
                return null;
            }

            if (!TryGetText(answers, DeviceField.HasBatteryDamage, out var batteryText)
                || !bool.TryParse(batteryText, out var hasBatteryDamage))
            {
                return null;
            }

            // Every answer was read successfully.
            return new Device
            {
                Brand = brand,
                Model = model,
                AgeInYears = age,
                Condition = condition,
                HasPersonalData = hasPersonalData,
                HasBatteryDamage = hasBatteryDamage
            };
        }

        // Reads one answer and trims it. False if it is missing, empty or spaces only.
        private static bool TryGetText(Dictionary<DeviceField, string> answers, DeviceField field, out string text)
        {
            text = answers.TryGetValue(field, out var value) ? value.Trim() : string.Empty;
            return text.Length > 0;
        }
    }
}
