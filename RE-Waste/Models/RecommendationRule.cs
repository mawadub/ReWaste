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

        //this method checks if a given device matches the criteria of this rule. It returns true if the rule is active and all non-null criteria match the device's properties.
        public bool Matches(Device device)
        {
            // Check if the rule is active and all non-null criteria match the device's properties
            return IsActive
            && (Condition == null || Condition == device.Condition)
            && (MinAgeInYears == null || device.AgeInYears >= MinAgeInYears)
            && (MaxAgeInYears == null || device.AgeInYears <= MaxAgeInYears)
            && (HasPersonalData == null || HasPersonalData == device.HasPersonalData)
            && (HasBatteryDamage == null || HasBatteryDamage == device.HasBatteryDamage);
        }
    }
}