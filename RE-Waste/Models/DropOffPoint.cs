using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // A place a user can take their phone; Should be managed by admin, searched by distance in R4.
    public class DropOffPoint
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(8)]
        public string Postcode { get; set; } = string.Empty;

        //will be used to calculate distance from user in R4, so we need to validate that they are within valid ranges for lat/lon
        [Range(-90, 90)]
        public double Latitude { get; set; }
        //using postcode.io for every request would be too slow
        [Range(-180, 180)]
        public double Longitude { get; set; }

        //helps to categorize drop-off points, e.g. "Retailer", "Recycling Center", "Charity Shop", etc.
        public DropOffPointType PointType { get; set; }

        [StringLength(200)]
        public string? OpeningHours { get; set; }

        [Url]
        [StringLength(200)]
        public string? Website { get; set; }

        //allow managers to deactivate drop-off points without deleting them
        public bool IsActive { get; set; } = true;
    }
}