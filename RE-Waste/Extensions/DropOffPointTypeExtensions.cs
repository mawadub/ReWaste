using RE_Waste.Models.Enums;

namespace RE_Waste.Extensions
{
    // Turns a DropOffPointType into the words a user should see
    public static class DropOffPointTypeExtensions
    {
        public static string ToDisplayName(this DropOffPointType type) => type switch
        {
            DropOffPointType.Charity => "Charity",
            DropOffPointType.RecyclingCentre => "Recycling centre",
            DropOffPointType.Retailer => "Retailer",
            DropOffPointType.Repair => "Repair",
            _ => type.ToString()
        };
    }
}
