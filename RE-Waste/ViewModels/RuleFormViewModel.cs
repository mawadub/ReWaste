using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.ViewModels
{
    public class RuleFormViewModel : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public RouteType Route { get; set; }

        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        [Range(0, 1000)]
        public int Priority { get; set; }

        public Condition? Condition { get; set; }

        [Range(0, 30)]
        public int? MinAgeInYears { get; set; }

        [Range(0, 30)]
        public int? MaxAgeInYears { get; set; }

        public bool? HasPersonalData { get; set; }

        public bool? HasBatteryDamage { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (MinAgeInYears.HasValue &&
                MaxAgeInYears.HasValue &&
                MinAgeInYears > MaxAgeInYears)
            {
                yield return new ValidationResult(
                    "Minimum age cannot be greater than maximum age.",
                    new[] { nameof(MinAgeInYears) });
            }
        }
    }
}
