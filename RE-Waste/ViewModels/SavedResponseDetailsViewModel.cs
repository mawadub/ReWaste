using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.ViewModels
{
    // U5: everything the saved result Details page needs
    public class SavedResponseDetailsViewModel
    {
        public SavedResponse Saved { get; set; } = null!;

        // False when the postcode district could not be looked up, so no places can be shown
        public bool LocationFound { get; set; }

        // Route1 first, then Route2 and Route3 only when they were saved
        public List<SavedRouteDetails> Routes { get; set; } = new();
    }

    // One recommended route with its reason and the nearest places for it
    public class SavedRouteDetails
    {
        public RouteType Route { get; set; }
        public string Reason { get; set; } = string.Empty;
        public IReadOnlyList<NearbyDropOffPoint> Places { get; set; } = new List<NearbyDropOffPoint>();
    }
}
