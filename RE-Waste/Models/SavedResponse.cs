using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // A recommendation a logged-in user saved (stages V1–V3). And listed in My devices (stage U4).
    public class SavedResponse
    {
        public int Id { get; set; }

        // Copy of the Device at the time of saving
        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        public int AgeInYears { get; set; }
        public Condition Condition { get; set; }
        public bool HasPersonalData { get; set; }
        public bool HasBatteryDamage { get; set; }

        // The postcode district the user entered when saving the recommendation. Used to show local recycling options.
        //only the first part of the postcode (e.g. "SW1A") is stored, not the full postcode (e.g. "SW1A 1AA"), to protect user privacy
        [Required]
        [StringLength(4)]
        public string PostcodeDistrict { get; set; } = string.Empty;

        // The recommended routes and reasons for the device. Route1 is always set; Route2 and Route3 are optional.
        public RouteType Route1 { get; set; }
        [Required]
        [StringLength(300)]
        public string Reason1 { get; set; } = string.Empty;

        public RouteType? Route2 { get; set; }
        [StringLength(300)]
        public string? Reason2 { get; set; }

        public RouteType? Route3 { get; set; }
        [StringLength(300)]
        public string? Reason3 { get; set; }

        // The date and time the recommendation was saved. Stored in UTC.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //navigation property
        // The user who saved the recommendation. This is a foreign key to the Users table.
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}