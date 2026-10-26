using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // ====================================================================
    // THE STATE OBJECT
    // This is the single "box" that the wizard remembers.
    // It holds the current step index and a dictionary of answers.
    // ====================================================================
    public class WizardState
    {
        // tracks where we are, 0 = first question, 1 = second etc
        public int Step { get; set; } = 0;

        // stores the answers in a dictionary to easily overwrite answers if the user goes back
        public Dictionary<DeviceField, string> Answers { get; set; } = new();


        // Stores the user's postcode for the results page.
        public string Postcode { get; set; } = string.Empty;

        // W6: true after a Change link on the review page, so Next goes straight back to the review.
        public bool ReturnToReview { get; set; }
    }
}
