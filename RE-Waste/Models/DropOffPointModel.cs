using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;
using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    public class DropOffPointModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(8)]
        public string Postcode { get; set; } = string.Empty;

        public DropOffPointType PointType { get; set; }

        [StringLength(200)]
        public string? OpeningHours { get; set; }

        [Url]
        [StringLength(200)]
        public string? Website { get; set; }

        // Used by Edit; Create should always set this to true.
        public bool IsActive { get; set; }
    }
}