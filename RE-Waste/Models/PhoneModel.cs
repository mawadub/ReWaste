using System.ComponentModel.DataAnnotations;

namespace RE_Waste.Models
{
    // One known phone model, e.g. Apple / iPhone 13. Filled from Data/Seed/phone-models.csv and editable later.
    public class PhoneModel
    {
        // What the wizard stores when the user's phone isn't in the list.
        public const string Other = "Other / not listed";

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ReleaseYear { get; set; }

        // Lets a manager hide a model without deleting it (old runs still point at it).
        public bool IsActive { get; set; } = true;
    }
}