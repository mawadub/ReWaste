using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // This file contains classes that are used as result types for interfaces 
    //An interface is a contract: it lists what a service can do, with no code for how. But it needs to return something, and these classes are the types that it returns.
    public class RouteRecommendation
    {
        //this class is used to return a recommendation for a route to take for a given item
        public RouteType Route { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class GeoLocation              
    {
        //used to store the latitude and longitude of a location after postcode lookup, but it doesn't need to be stored in the database, it's just used for returning results to the user
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class NearbyDropOffPoint       
    {
        //used to store a drop-off point and the distance from the user to that drop-off point, but it doesn't need to be stored in the database, it's just used for returning results to the user
        public DropOffPoint Point { get; set; } = null!;
        public double DistanceKm { get; set; }
    }
}
