using Microsoft.EntityFrameworkCore;
using RE_Waste.Data;
using RE_Waste.Models;
using RE_Waste.Models.Enums;

namespace RE_Waste.Services
{
    // R4: finds the nearest active drop-off points that suit a route, closest first.
    // It takes a GeoLocation, not a postcode - the caller does the postcode lookup first (R3).
    public class DropOffService : IDropOffService
    {
        // Which kinds of place suit each route. Kept in one place so it's easy to change if the group decides differently.
        // An array per route so a route can later accept more than one kind of place (e.g. Recycle could also use retailer take-back).
        private static readonly Dictionary<RouteType, DropOffPointType[]> PointTypesForRoute = new()
        {
            { RouteType.Donate, new[] { DropOffPointType.Charity } },
            { RouteType.ResaleOrTradeIn, new[] { DropOffPointType.Retailer } },
            { RouteType.Repair, new[] { DropOffPointType.Repair } },
            { RouteType.Recycle, new[] { DropOffPointType.RecyclingCentre } }
        };

        // Average radius of the earth, used by the Haversine formula
        private const double EarthRadiusKm = 6371;

        private readonly ApplicationDbContext _context;

        public DropOffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<NearbyDropOffPoint>> FindNearestAsync(RouteType route, GeoLocation from, int count = 5)
        {
            // Never return null: an empty list means "nothing found"
            if (count <= 0 || from == null || !PointTypesForRoute.TryGetValue(route, out var pointTypes))
            {
                return new List<NearbyDropOffPoint>();
            }

            // Only active points of the right kind. AsNoTracking because we only read them, which is faster.
            var points = await _context.DropOffPoints
                .AsNoTracking()
                .Where(p => p.IsActive && pointTypes.Contains(p.PointType))
                .ToListAsync();

            // The distance is worked out in memory, because SQL can't run the Haversine formula for us
            return points
                .Select(p => new NearbyDropOffPoint
                {
                    Point = p,
                    DistanceKm = DistanceKm(from.Latitude, from.Longitude, p.Latitude, p.Longitude)
                })
                .OrderBy(n => n.DistanceKm)
                .Take(count)
                .ToList();
        }

        // Haversine formula: the straight-line distance in km between two points on the surface.
        // Kept in its own method so it can be unit tested in Week 7.
        private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
