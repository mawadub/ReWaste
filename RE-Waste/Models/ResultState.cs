namespace RE_Waste.Models
{
    // ====================================================================
    // THE RESULT STATE (R6)
    // What the wizard worked out, kept in the session after Recommend.
    // The Results page reads it on every visit, so a refresh is safe, and
    // V1 will read it to save the result to the user's account.
    // It holds plain data only (no database objects), so it turns into
    // JSON cleanly, the same way WizardState does.
    // ====================================================================
    public class ResultState
    {
        // The phone as the engine saw it: brand, model, age, condition,
        // personal data and battery damage. Built by DeviceBuilder (W7).
        public Device Device { get; set; } = new();

        // Up to three routes from the engine, best first.
        // The reasons are kept exactly as the engine wrote them; never rewrite them.
        public List<RouteRecommendation> Routes { get; set; } = new();

        // The postcode the user typed on the postcode step (W4).
        // Used by the Results page to find the nearest places.
        public string Postcode { get; set; } = string.Empty;
    }
}
