using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Services
{
    //we use interface here to allow for dependency injection and testing
    //Task<...> and Async used because all three wait on something slow (the database or the internet). Async frees the web server to handle other users while waiting,
    //which matters for the 50 concurrent users requirement. The Async suffix is the .NET naming convention

    //Recommendation service is used to recommend a route for a given device, based on its properties. It returns a list of up to 3 recommendations, best first. If no recommendations are found, it falls back to Recycle.
    public interface IRecommendationService
    {
        // Up to 3 different routes, best first. Never empty (falls back to Recycle).
        Task<IReadOnlyList<RouteRecommendation>> RecommendAsync(Device device);
    }

    // Postcode service is used to look up the latitude and longitude of a given postcode. It returns null if the postcode or district isn't recognised.
    public interface IPostcodeService
    {
        // Null if the postcode or district isn't recognised.
        Task<GeoLocation?> LookupAsync(string postcode);
    }

    // Drop-off service is used to find the nearest active drop-off points for a given route and location. It returns a list of up to 5 points, closest first.
    public interface IDropOffService
    {
        // Nearest active points that suit the route, closest first.
        Task<IReadOnlyList<NearbyDropOffPoint>> FindNearestAsync(RouteType route, GeoLocation from, int count = 5);
    }
}
