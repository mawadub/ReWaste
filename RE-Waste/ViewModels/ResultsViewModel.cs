using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.ViewModels
{
    // R6: everything the Results page (Wizard/Results) shows.
    // Built fresh on every visit from the ResultState in the session.
    public class ResultsViewModel
    {
        // One entry per recommended route, best first.
        public List<ResultsRoute> Routes { get; set; } = new();

        // False when the postcode is missing or couldn't be looked up.
        // The page then shows the routes with a short note instead of places.
        public bool HasLocation { get; set; }

        // The user's postcode, shown in the intro text, e.g. "near EH1 2NG".
        public string Postcode { get; set; } = string.Empty;
    }

    // One route card: the route, the engine's reason and the nearest places.
    public class ResultsRoute
    {
        // e.g. RouteType.Donate. The view shows it with ToDisplayName().
        public RouteType Route { get; set; }

        // Word for word from the recommendation engine.
        public string Reason { get; set; } = string.Empty;

        // Up to three places, closest first. Empty when there's no location
        // or no suitable place is known.
        public IReadOnlyList<NearbyDropOffPoint> Places { get; set; } = new List<NearbyDropOffPoint>();
    }
}
