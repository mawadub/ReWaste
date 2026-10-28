using RE_Waste.Models.Enums;

namespace RE_Waste.Extensions
{
    // Turns a Condition into the words a user should see
    public static class ConditionExtensions
    {
        public static string ToDisplayName(this Condition condition) => condition switch
        {
            Condition.Working => "Working",
            Condition.PartlyWorking => "Partly working",
            Condition.NotWorking => "Not working",
            Condition.Damaged => "Damaged",
            _ => condition.ToString()
        };
    }
}
