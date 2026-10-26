namespace RE_Waste.ViewModels
{
    // W6: everything the review page (Wizard/Complete) shows.
    public class WizardReviewViewModel
    {
        // One row per active question, in the order the wizard asks them.
        public List<WizardReviewRow> Rows { get; set; } = new();

        // The postcode from W4, shown as the last row.
        public string Postcode { get; set; } = string.Empty;
    }

    // One question and its answer in plain words.
    public class WizardReviewRow
    {
        // The question text from the database, e.g. "What is the phone's condition?"
        public string Question { get; set; } = string.Empty;

        // The answer as the user would read it, e.g. "Partly working", not "PartlyWorking".
        public string Answer { get; set; } = string.Empty;

        // Position of the question in the wizard (0 = first). The Change link sends it to GoTo.
        public int Step { get; set; }
    }
}
