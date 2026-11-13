using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // A rule that recommends a Route for devices matching its criteria. A null criterion means "any".
    public class RecommendationRule
    {
        public int Id { get; set; }

        // The name of the rule, for internal use only. E.g. "Battery damage rule", "Age > 5 years rule".
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // The route to recommend if the rule matches. E.g. "Recycle", "Repair", "Donate".
        public RouteType Route { get; set; }

        // The reason for the recommendation, shown to the user. E.g. "Battery is damaged", "Device is too old".
        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        // Higher number wins when several rules match.
        public int Priority { get; set; }

        // Criteria for matching devices. Null means "any". 
        public Condition? Condition { get; set; }
        public int? MinAgeInYears { get; set; }
        public int? MaxAgeInYears { get; set; }
        public bool? HasPersonalData { get; set; }
        public bool? HasBatteryDamage { get; set; }

        // Allow managers to deactivate rules without deleting them.
        public bool IsActive { get; set; } = true;

        // If true, this rule always wins when its non-null criteria match,
        // regardless of similarity score. E.g. damaged battery → Recycle.
        public bool IsAbsolute { get; set; } = false;

        // Returns NULL if any non-null criterion contradicts the device (rule can't apply).
        // Otherwise returns how many non-null criteria match (0 = catch-all, higher = more specific).
        // "Contradicts" means the field is set on the rule AND differs from the device.
        public int? MatchScore(Device device)
        {
            if (!IsActive)
                return null; // Inactive rules never match.

            int score = 0;

            // Each block below: if the rule checks this field (non-null),
            // return null on mismatch, otherwise bump the score.

            if (Condition.HasValue)
            {
                if (Condition.Value != device.Condition) return null;
                score++;
            }

            if (MinAgeInYears.HasValue)
            {
                if (device.AgeInYears < MinAgeInYears.Value) return null;
                score++;
            }

            if (MaxAgeInYears.HasValue)
            {
                if (device.AgeInYears > MaxAgeInYears.Value) return null;
                score++;
            }

            if (HasPersonalData.HasValue)
            {
                if (HasPersonalData.Value != device.HasPersonalData) return null;
                score++;
            }

            if (HasBatteryDamage.HasValue)
            {
                if (HasBatteryDamage.Value != device.HasBatteryDamage) return null;
                score++;
            }

            // All non-null fields matched (or rule has no fields = catch-all).
            return score;
        }

        // Keep Matches() for backward compatibility / external callers.
        public bool Matches(Device device) => MatchScore(device).HasValue;
    }
}